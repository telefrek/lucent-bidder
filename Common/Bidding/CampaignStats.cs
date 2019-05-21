using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public class CampaignStats
    {
        long _clicks = 0L;
        long _wins = 0L;
        long _cpm = 0L;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double eCPC { get => Spend == 0 ? 0 : Clicks / Spend; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double CTR { get => _wins / Math.Max(1, Clicks); }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Spend { get => _cpm / 1000d; set { Interlocked.Add(ref _cpm, (long)(value * 10000L)); } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long Clicks { get => _clicks; set { Interlocked.Add(ref _clicks, value); } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long Wins { get => _wins; set { Interlocked.Add(ref _wins, value); } }

        static ConcurrentDictionary<string, CampaignStats> _budgets = new ConcurrentDictionary<string, CampaignStats>();

        /// <summary>
        /// Get te local budget for the given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static CampaignStats Get(string entityId) => _budgets.GetOrAdd(entityId, new CampaignStats { Id = entityId });

        /// <summary>
        /// Get all the budgets
        /// </summary>
        /// <returns></returns>
        public static CampaignStats[] GetAll() => _budgets.Values.ToArray();

        /// <summary>
        /// Gets/Sets the id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
    }

}