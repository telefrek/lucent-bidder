using Prometheus;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Counters for tracking message states in Prometheus
    /// </summary>
    public static class MessageCounters
    {
        /// <summary>
        /// Counter for tracking message errors
        /// </summary>
        /// <value></value>
        public static Counter ErrorCounter = Metrics.CreateCounter("message_errors", "Message error information", new CounterConfiguration
        {
            LabelNames = new string[] { "topic", "error" }
        });

        /// <summary>
        /// Track message processing latency
        /// </summary>
        /// <value></value>
        public static Histogram LatencyHistogram = Metrics.CreateHistogram("message_processing", "Message processing times by action", new HistogramConfiguration
        {
            LabelNames = new string[] { "topic", "action" },
            Buckets = MetricBuckets.LOW_LATENCY_10_MS,
        });
    }
}