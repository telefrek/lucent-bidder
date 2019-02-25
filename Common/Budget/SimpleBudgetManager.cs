using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Caching;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Logging;

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
        ILogger _log;

        /// <summary>
        /// Useless
        /// </summary>
        public SimpleBudgetManager(IMessageFactory messageFactory, IBudgetCache budgetCache, ILogger<SimpleBudgetManager> logger)
        {
            _budgetSubscriber = messageFactory.CreateSubscriber<BudgetEventMessage>(Topics.BUDGET, 0, messageFactory.WildcardFilter);
            _budgetSubscriber.OnReceive += HandleBudgetRequests;
            _budgetCache = budgetCache;
            _log = logger;
        }

        /// <summary>
        /// Handle events
        /// </summary>
        /// <param name="budgetEvent"></param>
        /// <returns></returns>
        async Task HandleBudgetRequests(BudgetEventMessage budgetEvent)
        {
            _log.LogInformation("Recieved budget event");

            // Only update our requests
            if (_requestCache.Remove(budgetEvent.Body.CorrelationId))
            {
                if (await _budgetCache.TryUpdateBudget(budgetEvent.Body.EntityId, budgetEvent.Body.Amount))
                    _log.LogInformation("Added {0} to {1}", budgetEvent.Body.Amount, budgetEvent.Body.EntityId);
                else
                    _log.LogWarning("Failed to add {0} to {1}", budgetEvent.Body.Amount, budgetEvent.Body.EntityId);
            }
            else
                _log.LogInformation("Request {0} not in cache", budgetEvent.Body.CorrelationId);
        }

        /// <inheritdoc/>
        public async Task GetAdditional(string id, IBudgetClient client) => await GetAdditional(1, id, client);

        /// <inheritdoc/>
        public async Task GetAdditional(double amount, string id, IBudgetClient client)
        {
            var correlationId = SequentialGuid.NextGuid();
            _requestCache.Add(correlationId);
            if (!await client.RequestBudget(id, amount, correlationId))
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