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
        public static LocalBudget Get(string entityId) => _budgets.GetOrAdd(entityId, new LocalBudget { Id = entityId, Last = 0d });

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
        /// Get the last value
        /// </summary>
        /// <value></value>
        public double Last { get; set; }

        /// <summary>
        /// Update the local budget for the entity
        /// </summary>
        /// <param name="value"></param>
        public double Update(double value) => (Interlocked.Add(ref _current, (long)(value * 10000L)) / 10000d) + Last;

        /// <summary>
        /// Check if the budget is exhausted
        /// </summary>
        /// <returns></returns>
        public bool IsExhausted() => Interlocked.Read(ref _current) / 10000d + Last <= 0d;

        /// <summary>
        /// Collect the budget and reset it to 0
        /// </summary>
        /// <returns></returns>
        public double Collect() => Interlocked.Exchange(ref _current, 0) / 10000d;
    }
}