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
        IAerospikeCache _cache;
        object _budgetSync = new object();
        bool localOnly;
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
        public BudgetCache(ILogger<BudgetCache> logger, ISerializationContext serializationContext, IAerospikeCache cache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _cache = cache;
            localOnly = _cache == null;
        }

        /// <inheritdoc/>
        public async Task<double> TryUpdateBudget(string key, double value)
        {
            var res = 0d;

            if (localOnly)
                res = 1d;
            else
                using (var context = _cacheLatency.CreateContext("inc"))
                {
                    res = await _cache.Inc(key, value, TimeSpan.FromHours(1));
                }

            if (res != double.NaN)
                _budgetValue.WithLabels(key).Set(res);

            return res;
        }

        /// <inheritdoc/>
        public async Task<double> TryGetBudget(string key)
        {
            var res = 0d;

            if (localOnly)
                res = 1d;
            else
                using (var context = _cacheLatency.CreateContext("get"))
                {
                    res = await _cache.Get(key);
                }

            if (res != double.NaN)
                _budgetValue.WithLabels(key).Set(res);

            return res;
        }
    }
}