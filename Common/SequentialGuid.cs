using System;
using System.Threading;

namespace Lucent.Common
{
    /// <summary>
    /// Generates Guids according to the RFC4122 specification.
    /// </summary>
    /// <remarks>
    /// <para>Based on the spec outlined at <a href="http://tools.ietf.org/rfc/rfc4122.txt"/></para>
    /// <para>Implementation uses 60 bits for the timestamp seed and 14 bits for the interval counter.  This provides a series
    /// covering 18,889,465,931,478,580,854,784 unique values before restarting.  The node is calculated based on a random
    /// series of 48 bits seeded from the current number of microseconds since the epoch (October 15, 1582).  Chances of
    /// a collision between node values is approximately 1 in 281,474,976,710,656.  Every second of seed difference between
    /// two sequences that have the same node value provides 16,383,000,000 unique values before collision.</para>
    /// <para>Local testing indicates the generator is capable of serving an average of 200-250 million Guids per second (in parallel).</para>
    /// </remarks>
    public static class SequentialGuid
    {
        static readonly object _syncLock;
        static readonly uint MASK = 0x3FFF; // mask to force a MOD 16,383
        static readonly byte[] _node; // 6 identifier bytes for the instance
        static readonly DateTime _epoch; // epoch as specified in the spec

        static uint _counter; // current value between 0-16,383
        static ulong _seed; // number of microseconds since epoch.

        static SequentialGuid()
        {
            _syncLock = new object();
            _counter = 0u;
            _epoch = new DateTime(1582, 11, 15);
            _node = new byte[6];
            _seed = (ulong)DateTime.UtcNow.Subtract(_epoch).Ticks;

            // Fill this with random bytes based on machine up-time and the thread id
            new Random(unchecked((int)(_seed & 0xFFFFFFFF) * Thread.CurrentThread.ManagedThreadId)).NextBytes(_node);
        }

        /// <summary>
        /// Creates the next sequential Guid in the sequence.
        /// </summary>
        /// <returns>A unique Guidd.</returns>
        public static Guid NextGuid()
        {
            var segment = 0u;
            var timestamp = 0UL;

            // Do the counter update in a lock to prevent multiple threads from getting the same value
            lock (_syncLock)
            {
                // Increment and then use the mask as a processor friendly modulus
                _counter = (uint)(++_counter & MASK);
                segment = _counter;
                if (segment == 0)
                    _seed++;

                timestamp = _seed;
            }

            return new Guid(
                (uint)(timestamp & 0xFFFFFFFF), // low bits (most volatile)
                (ushort)((timestamp >> 32) & 0xFFFF), // middle bits
                (ushort)(((timestamp >> 48) & 0xFFF) | 0x1000), // high bits with timestamp version flag
                (byte)((segment >> 8) | 0x80), // counter high bits and variant flag
                (byte)(segment & 0xFF), // counter low bits
                _node[0],
                _node[1],
                _node[2],
                _node[3],
                _node[4],
                _node[5]);
        }

        /// <summary>
        /// Resets the sequence at the current point in time.
        /// </summary>
        /// <remarks>
        /// This is included specifically to alleviate the highly unlikely possibility of having Guid overlap across
        /// a distributed system.
        /// </remarks>
        public static void Reset()
        {
            // Lock to prevent new guids during the reset
            lock (_syncLock)
            {
                // Reset the seed and counter values
                _seed = (ulong)DateTime.UtcNow.Subtract(_epoch).Ticks;
                _counter = 0;

                // Need to lock the node explicitly as it is used outside a lock statement during Guid generation.
                lock (_node)
                {
                    // Reset the node as this should only be called when there is an overlap.
                    new Random(unchecked((int)(_seed & 0xFFFFFFFF) * Thread.CurrentThread.ManagedThreadId)).NextBytes(_node);
                }
            }
        }
    }
}