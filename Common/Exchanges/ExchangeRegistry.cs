using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Exchange repository implementation
    /// </summary>
    public class ExchangeRegistry : IExchangeRegistry
    {
        ILogger<ExchangeRegistry> _log;
        FileSystemWatcher _watcher;
        Dictionary<string, AdExchange> _exchangeMap = new Dictionary<string, AdExchange>();
        IServiceProvider _provider;
        ExchangeConfig _config;

        object _syncLock = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="provider"></param>
        public ExchangeRegistry(IOptions<ExchangeConfig> config, ILogger<ExchangeRegistry> logger, IServiceProvider provider)
        {
            _log = logger;
            _provider = provider;
            _config = config.Value ?? new ExchangeConfig();
            if (!Directory.Exists(_config.ExchangeLocation))
                Directory.CreateDirectory(_config.ExchangeLocation);

            _watcher = new FileSystemWatcher(_config.ExchangeLocation, "*.dll");

            // Watch for exchanges that change
            _watcher.Changed += (o, e) =>
            {
                lock (_syncLock)
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                        LoadExchange(e.FullPath);
                }
            };

            // Watch for exchanges to remove
            _watcher.Deleted += (o, e) =>
            {
                lock (_syncLock)
                {
                    if (e.ChangeType == WatcherChangeTypes.Deleted)
                        RemoveExchange(e.FullPath);
                }
            };

            // Load any existing exchanges
            foreach (var file in new DirectoryInfo(_config.ExchangeLocation).GetFileSystemInfos("*.dll"))
                LoadExchange(file.FullName);

            _watcher.EnableRaisingEvents = true;
        }

        /// <inheritdoc/>
        public AdExchange GetExchange(HttpContext context)
        {
            var exchgId = context.Request.Query.FirstOrDefault(s => s.Key.Equals("exchg", StringComparison.InvariantCultureIgnoreCase));
            if (exchgId.Value.Any())
                return _exchangeMap.GetValueOrDefault(exchgId.Value.First(), null);
            return null;
        }

        void LoadExchange(string exchangePath)
        {
            _log.LogInformation("Loading exchange : {0}", exchangePath);
            try
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(exchangePath);
                if (asm != null)
                {
                    var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(AdExchange).IsAssignableFrom(t));
                    if (exchgType != null)
                    {
                        _log.LogInformation("Creating exchange type : {0}", exchgType.FullName);
                        var exchg = _provider.CreateInstance(exchgType) as AdExchange;
                        if (exchg != null)
                        {
                            _log.LogInformation("Loaded {0} ({1})", exchg.Name, exchg.ExchangeId);
                            exchg.Initialize(_provider);
                            _exchangeMap.Remove(exchangePath);
                            _exchangeMap.Add(exchangePath, exchg);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to load exchange : {0}", exchangePath);
            }
        }

        void RemoveExchange(string exchangePath)
        {
            _log.LogInformation("Removing exchange : {0}", exchangePath);
            try
            {
                _exchangeMap.Remove(exchangePath);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to remove : {0}", exchangePath);
            }
        }
    }
}