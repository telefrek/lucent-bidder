using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="bin"></param>
        /// <returns></returns>
        Task<double> Inc(string key, double value, TimeSpan expiration, string bin);

        /// <summary>
        /// Get the value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="bin"></param>
        /// <returns></returns>
        Task<double> Get(string key, string bin);

        /// <summary>
        /// Try to update the budget
        /// </summary>
        /// <param name="key"></param>
        /// <param name="inc"></param>
        /// <param name="max"></param>
        /// <param name="expiration"></param>
        /// <param name="bin"></param>
        /// <returns></returns>
        Task<bool> TryUpdateBudget(string key, double inc, double max, TimeSpan expiration, string bin);

        /// <summary>
        /// Try to update all the buckets
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiration"></param>
        /// <param name="buckets"></param>
        /// <returns></returns>
        Task<bool> TryUpdate(string key, TimeSpan expiration, params Tuple<string, DistributedValue>[] buckets);
    }

    /// <summary>
    /// This is a dumb class that I need to refactor away into nothingness...
    /// </summary>
    public class MockAspkCache : IAerospikeCache
    {
        static readonly IMemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15) });

        /// <inheritdoc/>
        public Task<double> Get(string key, string bin)
        {
            return Task.FromResult(((double?)_memcache.Get(key + "." + bin)).GetValueOrDefault(0d));
        }

        /// <inheritdoc/>
        public async Task<double> Inc(string key, double value, TimeSpan expiration, string bin)
        {
            return _memcache.Set(key + "." + bin, await Get(key, bin) + value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }

        /// <inheritdoc/>
        public Task<bool> TryUpdate(string key, TimeSpan expiration, params Tuple<string, DistributedValue>[] buckets)
        {
            foreach (var bucket in buckets)
            {
                var val = bucket.Item2.Reset();
                var valKey = key + bucket.Item1;
                    
                try
                {
                    bucket.Item2.Last = _memcache.Set(valKey, ((long?)_memcache.Get(valKey) ?? 0L) + val, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration
                    });
                }
                catch
                {
                    bucket.Item2.Inc(val);
                }
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdateBudget(string key, double inc, double max, TimeSpan expiration, string bin)
        {
            var cur = await Get(key, bin);
            if (cur + inc <= max)
            {
                await Inc(key, inc, expiration, bin);
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
        public void Dispose() { }

        /// <inheritdoc/>
        public async Task<double> Inc(string key, double value, TimeSpan expiration, string bin)
        {
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "inc"))
                try
                {
                    var res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = (int)expiration.TotalSeconds },
                    default(CancellationToken), new Key("lucent", "lucent", key),
                        Operation.Add(new Bin(bin, (int)(value * 10000))), Operation.Get(bin));
                    if (res != null)
                        return res.GetInt(bin) / 10000d;
                }
                catch (AerospikeException e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "lucent", "inc", e.Result.ToString()).Inc();
                    _log.LogError(e, "Error during increment");
                }

            return double.NaN;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdateBudget(string key, double inc, double max, TimeSpan expiration, string bin)
        {
            var op = "get";
            try
            {
                Record res = null;
                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "budget", "get"))
                    res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", bin, key));

                if (res == null)
                {
                    op = "update";
                    using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "budget", "update"))
                        res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = (int)expiration.TotalSeconds },
                        default(CancellationToken), new Key("lucent", "budget", key),
                            Operation.Add(new Bin(bin, (int)(inc * 10000))), Operation.Get(bin));

                    return res != null;
                }

                if (res.GetInt(bin) / 10000d + inc <= max)
                {
                    using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "budget", "update"))
                        res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = res.TimeToLive },
                        default(CancellationToken), new Key("lucent", "budget", key),
                            Operation.Add(new Bin(bin, (int)(inc * 10000))), Operation.Get(bin));

                    return res != null;
                }
            }
            catch (AerospikeException e)
            {
                StorageCounters.ErrorCounter.WithLabels("aerospike", "budget", op, e.Result.ToString()).Inc();
                _log.LogError(e, "Error during budget update");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<double> Get(string key, string bin)
        {
            try
            {
                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "get"))
                {
                    var res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", "lucent", key));
                    if (res != null)
                        return res.GetInt(bin) / 10000d;
                }
            }
            catch (AerospikeException e)
            {
                StorageCounters.ErrorCounter.WithLabels("aerospike", "lucent", "get", e.Result.ToString()).Inc();
                _log.LogError(e, "Error during increment");
            }

            return double.NaN;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate(string key, TimeSpan expiration, params Tuple<string, DistributedValue>[] buckets)
        {
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "update"))
            {
                var operations = buckets.Select(b => new { Name = b.Item1, Val = b.Item2.Reset() }).ToArray();
                try
                {
                    var res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = (int)expiration.TotalSeconds },
                    default(CancellationToken), new Key("lucent", "lucent", key),
                        operations.Select(o => Operation.Add(new Bin(o.Name, o.Val))).Concat(new Operation[] { Operation.Get() }).ToArray());

                    if (res != null)
                    {
                        foreach (var bucket in buckets)
                            if (res.bins.ContainsKey(bucket.Item1))
                                bucket.Item2.Last = res.GetLong(bucket.Item1);

                        return true;
                    }
                }
                catch (AerospikeException e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "lucent", "update", e.Result.ToString()).Inc();
                    _log.LogError(e, "Error during tryUpdate");

                }
            }

            return false;
        }
    }
}