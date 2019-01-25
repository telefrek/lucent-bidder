using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Basic implementation
    /// </summary>
    public class SimpleBudgetClient : IBudgetClient
    {
        ILogger<SimpleBudgetClient> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public SimpleBudgetClient(ILogger<SimpleBudgetClient> logger)
        {
            _log = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<bool> RequestBudget(string entityId, decimal amount)
        {
            return await Task.FromResult(false);
        }
    }
}