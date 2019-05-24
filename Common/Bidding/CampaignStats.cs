using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public class CampaignStats
    {
        /// <summary>
        /// Conversions
        /// </summary>
        /// <returns></returns>
        public DistributedValue Conversions = new DistributedValue();

        /// <summary>
        /// Wins
        /// </summary>
        /// <returns></returns>
        public DistributedValue Wins = new DistributedValue();

        /// <summary>
        /// CPM
        /// </summary>
        /// <returns></returns>
        public DistributedValue CPM = new DistributedValue();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double eCPC { get => CPM.GetRaw() == 0 ? 0 : Conversions.GetRaw() / CPM.GetDouble(); }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double CTR { get => Wins.GetDouble() * 1d / Math.Max(1, Conversions.GetRaw()); }

        static ConcurrentDictionary<string, CampaignStats> _stats = new ConcurrentDictionary<string, CampaignStats>();
        internal DistributedValue c;

        /// <summary>
        /// Get te local budget for the given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static CampaignStats Get(string entityId) => _stats.GetOrAdd(entityId, new CampaignStats { Id = entityId });

        /// <summary>
        /// Get all the budgets
        /// </summary>
        /// <returns></returns>
        public static CampaignStats[] GetAll() => _stats.Values.ToArray();

        /// <summary>
        /// Gets/Sets the id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
    }

}