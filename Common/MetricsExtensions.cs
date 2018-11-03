using System;
using System.Diagnostics;
using Prometheus;

namespace Lucent.Common
{
    /// <summary>
    /// Timing context for histograms
    /// </summary>
    public class HistogramTimingContext : IDisposable
    {
        Stopwatch _timer;
        Histogram.Child _target;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="child"></param>
        public HistogramTimingContext(Histogram.Child child)
        {
            _target = child;
            _timer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Dispose implementation
        /// </summary>
        public void Dispose() => _target.Observe(_timer.GetMilliseconds());
    }

    /// <summary>
    /// Extensions for prometheus metrics
    /// </summary>
    public static class MetricsExtensions
    {
        /// <summary>
        /// Creates a timing context for the given histogram / label combination
        /// </summary>
        /// <param name="target"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static HistogramTimingContext CreateContext(this Histogram target, params string[] labels) 
            => new HistogramTimingContext(target.Labels(labels));
    }
}