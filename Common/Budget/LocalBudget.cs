using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Hold local budget updates
    /// </summary>
    public class LocalBudget
    {
        static ConcurrentDictionary<string, LocalBudget> _budgets = new ConcurrentDictionary<string, LocalBudget>();

        /// <summary>
        /// Get te local budget for the given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static LocalBudget Get(string entityId) => _budgets.GetOrAdd(entityId, new LocalBudget { Id = entityId });

        /// <summary>
        /// Get all the budgets
        /// </summary>
        /// <returns></returns>
        public static LocalBudget[] GetAll() => _budgets.Values.ToArray();

        long _current = 0L;

        /// <summary>
        /// Gets/Sets the id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// Hourly spend value
        /// </summary>
        /// <value></value>
        public DistributedValue Budget { get; set; } = new DistributedValue();

        /// <summary>
        /// Action counts for campaign
        /// </summary>
        /// <value></value>
        public DistributedValue ActionLimit { get; set; } = new DistributedValue();

        /// <summary>
        /// Daily revenue for the campaign
        /// </summary>
        /// <value></value>
        public DistributedValue DailyRevenueSpend { get; set; } = new DistributedValue();

        /// <summary>
        /// Total revenue for this campaign
        /// </summary>
        /// <value></value>
        public DistributedValue TotalRevenueSpend { get; set; } = new DistributedValue();
    }
}