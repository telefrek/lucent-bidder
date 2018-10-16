using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Exchange repository implementation
    /// </summary>
    public class ExchangeRegistry : IExchangeRegistry
    {
        ILogger<ExchangeRegistry> _log;
        FileSystemWatcher _watcher;
        List<IAdExchange> _exchanges = new List<IAdExchange>();
        Dictionary<string, IAdExchange> _exchangeMap = new Dictionary<string, IAdExchange>();
        IServiceProvider _provider;

        object _syncLock = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="provider"></param>
        public ExchangeRegistry(ILogger<ExchangeRegistry> logger, IServiceProvider provider)
        {
            _log = logger;
            _provider = provider;
            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            if (!Directory.Exists(exchangePath))
                Directory.CreateDirectory(exchangePath);

            _watcher = new FileSystemWatcher(exchangePath, "*.dll");

            // Watch for exchanges that change
            _watcher.Changed += (o, e) =>
            {
                lock (_syncLock)
                {
                    LoadExchange(e.FullPath);
                }
            };

            // Watch for exchanges to remove
            _watcher.Deleted += (o, e) =>
            {
                lock (_syncLock)
                {
                    RemoveExchange(e.FullPath);
                }
            };

            // Load any existing exchanges
            foreach (var file in new DirectoryInfo(exchangePath).GetFileSystemInfos("*.dll"))
                LoadExchange(file.FullName);

            _watcher.EnableRaisingEvents = true;
        }

        void LoadExchange(string exchangePath)
        {
            _log.LogInformation("Loading exchange : {0}", exchangePath);
            try
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(exchangePath);
                if (asm != null)
                {
                    var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(IAdExchange).IsAssignableFrom(t));
                    if (exchgType != null)
                    {
                        _log.LogInformation("Creating exchange type : {0}", exchgType.FullName);
                        var exchg = _provider.CreateInstance(exchgType) as IAdExchange;
                        if (exchg != null)
                        {
                            _log.LogInformation("Loaded {0} ({1})", exchg.Name, exchg.ExchangeId);
                            exchg.Initialize(_provider);
                            _exchangeMap.Remove(exchangePath);
                            _exchangeMap.Add(exchangePath, exchg);
                            _exchanges = _exchangeMap.Values.ToList();
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
                _exchanges = _exchangeMap.Values.ToList();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to remove : {0}", exchangePath);
            }
        }

        /// <summary>
        /// Gets the current excchanges
        /// </summary>
        public List<IAdExchange> Exchanges => _exchanges;
    }
}