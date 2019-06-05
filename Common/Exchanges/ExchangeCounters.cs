using Prometheus;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Set of common exchange counters
    /// </summary>
    public static class ExchangeCounters
    {
        /// <summary>
        /// Exchange latency counter
        /// </summary>
        /// <value></value>
        public static Histogram ExchangeLatency = Metrics.CreateHistogram("exchange_bidder_latency", "Exchange bidder latency", new HistogramConfiguration
        {
            LabelNames = new string[] { "exchange" },
            Buckets = MetricBuckets.LOW_LATENCY_10_MS
        });


    }
}