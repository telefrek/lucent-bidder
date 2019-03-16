using System;
using System.IO;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetCache : IBudgetCache
    {
        ILogger _log;
        ISerializationContext _serializationContext;
        IDistributedCache _cache;
        object _budgetSync = new object();
        MemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(1), SizeLimit = 1024 * 1024 * 64 });


        /// <summary>
        /// Metric for tracking query latencies
        /// </summary>
        /// <value></value>
        static Histogram _cacheLatency = Metrics.CreateHistogram("cache_latency", "Latency for Caching queries", new HistogramConfiguration
        {
            LabelNames = new string[] { "method" },
            Buckets = new double[] { 0.0005, 0.001, 0.002, 0.005, 0.010, 0.015, 0.020, 0.025, 0.05 },
        });

        static Gauge _budgetValue = Metrics.CreateGauge("current_budget", "The current budget for an item", new GaugeConfiguration
        {
            LabelNames = new string[] { "entity" }
        });

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializationContext"></param>
        /// <param name="cache"></param>
        public BudgetCache(ILogger<BudgetCache> logger, ISerializationContext serializationContext, IDistributedCache cache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _cache = cache;
        }

        /// <inheritdoc/>
        public async Task<double> GetBudget(string key)
        {
            var res = (byte[])_memcache.Get(key);
            if (res == null)
            {
                using (var context = _cacheLatency.CreateContext("get"))
                {
                    res = await _cache.GetAsync(key);
                }
                if (res != null)
                    _memcache.Set(key, res, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1), Size = res.Length });
            }
            return BitConverter.Int64BitsToDouble(BitConverter.ToInt64(res ?? BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(0d)), 0));
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemove(string key)
        {
            using (var context = _cacheLatency.CreateContext("remove"))
            {
                await _cache.RemoveAsync(key);
            }
            _memcache.Remove(key);
            return true;
        }

        /// <inheritdoc/>
        public async Task<T> TryRetrieve<T>(string key) where T : class, new()
        {
            var bytes = (byte[])_memcache.Get(key);
            if (bytes == null)
            {
                using (var context = _cacheLatency.CreateContext("get"))
                {
                    bytes = await _cache.GetAsync(key);
                }
                if (bytes != null)
                    _memcache.Set(key, bytes, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1), Size = bytes.Length });
            }
            return bytes != null ? await _serializationContext.ReadFrom<T>(new MemoryStream((byte[])bytes), false, SerializationFormat.PROTOBUF) : default(T);
        }

        /// <inheritdoc/>
        public async Task<bool> TryStore<T>(T instance, string key, TimeSpan expiration) where T : class, new()
        {
            var raw = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);

            using (var context = _cacheLatency.CreateContext("set"))
            {
                await _cache.SetAsync(key, raw, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = expiration,
                });
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate<T>(T instance, string key, TimeSpan expiration) where T : class, new()
        {
            var raw = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);

            using (var context = _cacheLatency.CreateContext("set"))
            {
                await _cache.SetAsync(key, raw, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = expiration
                });
            }
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdateBudget(string key, double value)
        {
            var current = await GetBudget(key);

            current += value;

            using (var context = _cacheLatency.CreateContext("set"))
            {
                await _cache.SetAsync(key, BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(current)), new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(1),
                });
            }

            _budgetValue.WithLabels(key).Set(current);

            return true;
        }
    }
}