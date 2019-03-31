namespace Lucent.Common
{
    /// <summary>
    /// Common bucket definitions
    /// </summary>
    public static class MetricBuckets
    {
        /// <summary>
        /// Track buckets on actions that should be exceedingly low latency (sub 5 ms)
        /// </summary>
        /// <value></value>
        public static double[] LOW_LATENCY_5_MS = new double[] { 0.0005, 0.001, 0.0015, 0.002, 0.0025, 0.003, 0.0035, 0.004, 0.0045, 0.005 };

        /// <summary>
        /// Track buckets against API latency targets
        /// </summary>
        /// <value></value>
        public static double[] API_LATENCY = new double[] { 0.001, 0.0025, 0.005, 0.0075, 0.01, 0.0125, 0.015, 0.02, 0.025, 0.05 };

        /// <summary>
        /// Track buckets on actions that should be lower latency (sub 10 ms)
        /// </summary>
        /// <value></value>
        public static double[] LOW_LATENCY_10_MS = new double[] { 0.001, 0.0015, 0.002, 0.0025, 0.003, 0.004, 0.005, 0.006, 0.0075, 0.01 };
    }
}