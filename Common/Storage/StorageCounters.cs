using Prometheus;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Common storage counters
    /// </summary>
    public static class StorageCounters
    {
        /// <summary>
        /// Counter for tracking message errors
        /// </summary>
        /// <value></value>
        public static Counter ErrorCounter = Metrics.CreateCounter("storage_errors", "Storage error information", new CounterConfiguration
        {
            LabelNames = new string[] { "system", "keyspace", "error" }
        });

        /// <summary>
        /// Track message processing latency
        /// </summary>
        /// <value></value>
        public static Histogram LatencyHistogram = Metrics.CreateHistogram("storage_latency", "Storage processing times by action", new HistogramConfiguration
        {
            LabelNames = new string[] { "system", "keyspace", "query" },
            Buckets = MetricBuckets.LOW_LATENCY_5_MS
        });
    }
}