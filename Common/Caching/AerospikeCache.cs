using System;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// Configuration for hte Aerospike Cache
    /// </summary>
    public class AerospikeConfig
    {
        /// <summary>
        /// Gets/Sets the ServiceName
        /// </summary>
        /// <value></value>
        public string ServiceName { get; set; } = "aspk-cache.lucent.svc";

        /// <summary>
        /// Gets/Sets the port
        /// </summary>
        /// <value></value>
        public int Port { get; set; } = 3000;
    }

    /// <summary>
    /// Wrapper
    /// </summary>
    public interface IAerospikeCache
    {
        /// <summary>
        /// Increment the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task<double> Inc(string key, double value, TimeSpan expiration);

        /// <summary>
        /// Get the value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<double> Get(string key);
    }

    /// <summary>
    /// Implementation of the distributed cache using Aerospike
    /// </summary>
    public class AerospikeCache : IAerospikeCache
    {
        AerospikeConfig _config;
        AsyncClient _client;
        Policy _readPolicy;
        WritePolicy _writePolicy;
        ILogger _log;

        /// <summary>
        /// Default Dependency Injection construcctor
        /// </summary>
        public AerospikeCache(ILogger<AerospikeCache> logger)
        {
            _log = logger;
            var policy = new AsyncClientPolicy
            {
                asyncMaxCommands = 1024,
            };

            _client = new AsyncClient(policy, "aspk-cache.lucent.svc", 3000);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public async Task<double> Inc(string key, double value, TimeSpan expiration)
        {
            try
            {
                var res = await _client.Operate(new WritePolicy { expiration = (int)expiration.TotalSeconds },
                default(CancellationToken), new Key("lucent", "lucent", key),
                    Operation.Add(new Bin("budget", (int)(value * 10000))), Operation.Get("budget"));
                if (res != null)
                    return res.GetInt("budget") / 10000d;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error during increment");
            }

            return double.NaN;
        }

        /// <inheritdoc/>
        public async Task<double> Get(string key)
        {
            try
            {
                var res = await _client.Get(new Policy { }, default(CancellationToken), new Key("lucent", "lucent", key));
                if (res != null)
                    return res.GetInt("budget") / 10000d;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error during increment");
            }

            return double.NaN;
        }
    }
}