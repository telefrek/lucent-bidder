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
        Task<decimal> GetRemaining(string id);

        /// <summary>
        /// Get additional budget, unbounded
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task GetAdditional(string id);

        /// <summary>
        /// Get up to an additional amount
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task GetAdditional(decimal amount, string id);

        /// <summary>
        /// Check if the budget is exhausted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsExhausted(string id);

        /// <summary>
        /// Try to spend the amount
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> TrySpend(decimal amount, string id);
    }
}