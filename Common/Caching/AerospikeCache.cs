using System;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;
using Lucent.Common.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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

        /// <summary>
        /// Try to update the budget
        /// </summary>
        /// <param name="key"></param>
        /// <param name="inc"></param>
        /// <param name="max"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task<bool> TryUpdateBudget(string key, double inc, double max, TimeSpan expiration);
    }

    /// <summary>
    /// This is a dumb class that I need to refactor away into nothingness...
    /// </summary>
    public class MockAspkCache : IAerospikeCache
    {
        static readonly MemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15) });

        /// <inheritdoc/>
        public Task<double> Get(string key)
        {
            return Task.FromResult(((double?)_memcache.Get(key)).GetValueOrDefault(0d));
        }

        /// <inheritdoc/>
        public async Task<double> Inc(string key, double value, TimeSpan expiration)
        {
            return _memcache.Set(key, await Get(key) + value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdateBudget(string key, double inc, double max, TimeSpan expiration)
        {
            var cur = await Get(key);
            if(cur + inc <= max)
            {
                await Inc(key, inc, expiration);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Implementation of the distributed cache using Aerospike
    /// </summary>
    public class AerospikeCache : IAerospikeCache
    {
        AerospikeConfig _config;
        ILogger _log;

        /// <summary>
        /// Default Dependency Injection construcctor
        /// </summary>
        public AerospikeCache(ILogger<AerospikeCache> logger)
        {
            _log = logger;
        }

        /// <inheritdoc/>
        public void Dispose() {}

        /// <inheritdoc/>
        public async Task<double> Inc(string key, double value, TimeSpan expiration)
        {
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "inc"))
                try
                {
                    var res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = (int)expiration.TotalSeconds },
                    default(CancellationToken), new Key("lucent", "lucent", key),
                        Operation.Add(new Bin("budget", (int)(value * 10000))), Operation.Get("budget"));
                    if (res != null)
                        return res.GetInt("budget") / 10000d;
                }
                catch (Exception e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "lucent", e.GetType().Name).Inc();
                    _log.LogError(e, "Error during increment");
                }

            return double.NaN;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdateBudget(string key, double inc, double max, TimeSpan expiration)
        {
            try
            {
                Record res = null;
                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "budget", "get"))
                    res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", "budget", key));

                if (res == null)
                {
                    using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "budget", "update"))
                        res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = (int)expiration.TotalSeconds },
                        default(CancellationToken), new Key("lucent", "budget", key),
                            Operation.Add(new Bin("budget", (int)(inc * 10000))), Operation.Get("budget"));

                    return res != null;
                }

                if (res.GetInt("budget") / 10000d + inc <= max)
                {
                    using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "budget", "update"))
                        res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = res.TimeToLive },
                        default(CancellationToken), new Key("lucent", "budget", key),
                            Operation.Add(new Bin("budget", (int)(inc * 10000))), Operation.Get("budget"));

                    return res != null;
                }
            }
            catch (Exception e)
            {
                StorageCounters.ErrorCounter.WithLabels("aerospike", "budget", e.GetType().Name).Inc();
                _log.LogError(e, "Error during budget update");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<double> Get(string key)
        {
            try
            {
                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "get"))
                {
                    var res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", "lucent", key));
                    if (res != null)
                        return res.GetInt("budget") / 10000d;
                }
            }
            catch (Exception e)
            {
                StorageCounters.ErrorCounter.WithLabels("aerospike", "lucent", e.GetType().Name).Inc();
                _log.LogError(e, "Error during increment");
            }

            return double.NaN;
        }
    }
}