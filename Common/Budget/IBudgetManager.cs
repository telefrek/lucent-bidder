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
        /// Invocation for when budget events are recieved
        /// </summary>
        /// <value></value>
        Func<BudgetEvent, Task> OnStatusChanged { get; set; }

        /// <summary>
        /// Request additional budget for the entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task RequestAdditional(string entityId);
    }
}