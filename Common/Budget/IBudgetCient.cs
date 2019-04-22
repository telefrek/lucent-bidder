using System;
using System.Threading.Tasks;
using Lucent.Common.Entities;

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
        /// <param name="entityType"></param>
        /// <returns></returns>
        Task<bool> RequestBudget(string entityId, EntityType entityType);
    }
}