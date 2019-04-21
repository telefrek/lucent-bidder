using System;
using System.Threading.Tasks;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Handler for budget events
    /// </summary>
    public class BudgetEventHandler
    {
        /// <summary>
        /// Function to test for match
        /// </summary>
        /// <returns></returns>
        public Func<BudgetEvent, bool> IsMatch { get; set; } = (e) => { return false; };

        /// <summary>
        /// Function for handling the event asynchronously
        /// </summary>
        /// <returns></returns>
        public Func<BudgetEvent, Task> HandleAsync { get; set; } = (e) => { return Task.CompletedTask; };
    }
}