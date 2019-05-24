using System.Threading;

namespace Lucent.Common
{
    /// <summary>
    /// Shim class to handle distributed values
    /// </summary>
    public class DistributedValue
    {
        long _current = 0L;

        /// <summary>
        /// Get/Set the last value
        /// </summary>
        /// <value></value>
        public long Last { get; set; }

        /// <summary>
        /// Get the value as a double
        /// </summary>
        /// <returns></returns>
        public double GetDouble() => (Interlocked.Read(ref _current) + Last) / 1000d;

        /// <summary>
        /// Get the raw value
        /// </summary>
        /// <returns></returns>
        public long GetRaw() => Interlocked.Read(ref _current) + Last;

        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The total value</returns>
        public double Inc(double value) => (Interlocked.Add(ref _current, (long)(value * 10000L)) + Last) / 10000d;

        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The total value</returns>
        public long Inc(long value) => Interlocked.Add(ref _current, value) + Last;

        /// <summary>
        /// Reset the value
        /// </summary>
        /// <returns>The aggregation of this process</returns>
        public long Reset() => Interlocked.Exchange(ref _current, 0L);
    }
}