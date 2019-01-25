using System.Threading.Tasks;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Client for managing budget API calls
    /// </summary>
    public interface IBudgetClient
    {
        /// <summary>
        /// Register a budget request
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<bool> RequestBudget(string entityId, decimal amount);
    }
}