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
            Buckets = new double[] { 0.005, 0.010, 0.015, 0.025, 0.050, 0.075, 0.100, 0.125, 0.150, 0.200, 0.25, 0.5, 0.75, 1.0 },
        });
    }
}