using System.Threading.Tasks;
using Lucent.Common.Storage;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Manage budgets
    /// </summary>
    public interface IBudgetManager
    {
        /// <summary>
        /// Get the remaining budget
        /// </summary>
        /// <returns></returns>
        Task<decimal> GetRemaining();

        /// <summary>
        /// Get additional budget, unbounded
        /// </summary>
        /// <returns></returns>
        Task<decimal> GetAdditional();

        /// <summary>
        /// Get up to an additional amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<decimal> GetAdditional(decimal amount);

        /// <summary>
        /// Check if the budget is exhausted
        /// </summary>
        /// <returns></returns>
        bool IsExhausted();

        /// <summary>
        /// Try to spend the amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<bool> TrySpend(decimal amount);
    }
}