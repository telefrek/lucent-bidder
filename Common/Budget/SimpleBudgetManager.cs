using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Caching;
using Lucent.Common.Messaging;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// This is terrible
    /// </summary>
    public class SimpleBudgetManager : IBudgetManager
    {
        HashSet<Guid> _requestCache = new HashSet<Guid>();
        IBudgetCache _budgetCache;
        IMessageSubscriber<BudgetEventMessage> _budgetSubscriber;
        IBudgetClient _budgetClient;

        /// <summary>
        /// Useless
        /// </summary>
        public SimpleBudgetManager(IMessageFactory messageFactory, IBudgetClient budgetClient, IBudgetCache budgetCache)
        {
            _budgetSubscriber = messageFactory.CreateSubscriber<BudgetEventMessage>("budget", 0, messageFactory.WildcardFilter);
            _budgetSubscriber.OnReceive = HandleBudgetRequests;
            _budgetClient = budgetClient;
            _budgetCache = budgetCache;
        }

        /// <summary>
        /// Handle events
        /// </summary>
        /// <param name="budgetEvent"></param>
        /// <returns></returns>
        async Task HandleBudgetRequests(BudgetEventMessage budgetEvent)
        {
            // Only update our requests
            if (_requestCache.Remove(budgetEvent.Body.CorrelationId))
                await _budgetCache.TryUpdateBudget(budgetEvent.Body.EntityId, budgetEvent.Body.Amount);
        }

        /// <inheritdoc/>
        public async Task GetAdditional(string id) => await GetAdditional(1, id);

        /// <inheritdoc/>
        public async Task GetAdditional(double amount, string id)
        {
            var correlationId = SequentialGuid.NextGuid();
            _requestCache.Add(correlationId);
            if (!await _budgetClient.RequestBudget(id, amount, correlationId))
                _requestCache.Remove(correlationId);
        }

        /// <inheritdoc/>
        public async Task<double> GetRemaining(string id) => await _budgetCache.GetBudget(id);

        /// <inheritdoc/>
        public bool IsExhausted(string id) => GetRemaining(id).Result <= 0;

        /// <inheritdoc/>
        public async Task<bool> TrySpend(double amount, string id) => await _budgetCache.TryUpdateBudget(id, amount);
    }
}