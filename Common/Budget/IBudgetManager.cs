using System;
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
        /// Register a handler
        /// </summary>
        /// <param name="handler"></param>
        void RegisterHandler(BudgetEventHandler handler);

        /// <summary>
        /// Request additional budget for the entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task RequestAdditional(string entityId);
    }
}