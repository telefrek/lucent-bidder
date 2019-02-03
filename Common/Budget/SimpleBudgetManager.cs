using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lucent.Common.Messaging;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// This is terrible
    /// </summary>
    public class SimpleBudgetManager : IBudgetManager
    {
        ConcurrentDictionary<string, double> _budgets = new ConcurrentDictionary<string, double>();
        IMessageSubscriber<BudgetEventMessage> _budgetSubscriber;
        IBudgetClient _budgetClient;

        /// <summary>
        /// Useless
        /// </summary>
        public SimpleBudgetManager(IMessageFactory messageFactory, IBudgetClient budgetClient)
        {
            _budgetSubscriber = messageFactory.CreateSubscriber<BudgetEventMessage>("budget", 0, messageFactory.WildcardFilter);
            _budgetSubscriber.OnReceive = HandleBudgetRequests;
            _budgetClient = budgetClient;
        }

        /// <summary>
        /// Handle events
        /// </summary>
        /// <param name="budgetEvent"></param>
        /// <returns></returns>
        async Task HandleBudgetRequests(BudgetEventMessage budgetEvent)
        {
            _budgets.AddOrUpdate(budgetEvent.Body.EntityId, budgetEvent.Body.Amount, (eid, o) => o += budgetEvent.Body.Amount);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task GetAdditional(string id) => await GetAdditional(1, id);

        /// <inheritdoc/>
        public async Task GetAdditional(double amount, string id) => await _budgetClient.RequestBudget(id, amount);

        /// <inheritdoc/>
        public async Task<double> GetRemaining(string id)
        {
            return await Task.FromResult(_budgets.GetOrAdd(id, 0));
        }

        /// <inheritdoc/>
        public bool IsExhausted(string id) => _budgets.GetOrAdd(id, 0) <= 0;

        /// <inheritdoc/>
        public async Task<bool> TrySpend(double amount, string id)
        {
            return await Task.FromResult(_budgets.AddOrUpdate(id, -amount, (i, o) => o - amount) >= 0);
        }
    }
}