using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// Manage source records
    /// </summary>
    public class SourceCache
    {
        static object _syncLock = new object();
        static HashSet<string> _names = new HashSet<string>();
        static Random _rng = new Random();

        static IMemoryCache _memCache = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromSeconds(10),
            SizeLimit = 100
        });

        /// <summary>
        /// Scan the current cache
        /// </summary>
        /// <returns>The string representation of sources by order</returns>
        public static string Scan()
        {
            var sb = new StringBuilder();
            var entries = new List<SourceStat>();
            lock (_syncLock)
            {
                foreach (var entry in _names)
                {
                    var s = _memCache.Get<SourceStat>(entry);
                    if (s != null)
                    {
                        entries.Add(s);
                    }
                }
            }

            sb.AppendLine("Sites:\n");
            sb.AppendLine("{0,15} ({1,10}) : {2}".FormatWith("bids", "aCPM", "name"));

            foreach (var entry in entries.OrderByDescending(s => s.Total))
                sb.AppendLine("{0,15} ({1,10:#.0000}) : {2}".FormatWith(entry.Count, entry.Total / entry.Count, entry.Name));

            return sb.ToString();
        }

        /// <summary>
        /// record an entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cpm"></param>
        public static void Sample(string id, double cpm)
        {
            // Sample 1/1000 of the traffic for now
            if (_rng.NextDouble() < .01)
                lock (_syncLock)
                {
                    var stat = _memCache.Get<SourceStat>(id) ?? new SourceStat { Name = id, Count = 0 };
                    stat.Count += 1;
                    stat.Total += cpm;
                    _names.Add(id);
                    _memCache.Set(id, stat, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                        Size = 1,
                    }.RegisterPostEvictionCallback((k, v, r, s) =>
                    {
                        _names.Remove((string)k);
                    }));
                }
        }
    }

    class SourceStat
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public double Total { get; set; }
    }
}