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
        HashSet<string> _entities = new HashSet<string>();
        HashSet<Guid> _ids = new HashSet<Guid>();

        IBudgetCache _budgetCache;
        IMessageSubscriber<BudgetEventMessage> _budgetSubscriber;
        ILogger _log;
        IBudgetClient _budgetClient;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="budgetCache"></param>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        public SimpleBudgetManager(IMessageFactory messageFactory, IBudgetCache budgetCache, ILogger<SimpleBudgetManager> logger, IBudgetClient client)
        {
            _budgetSubscriber = messageFactory.CreateSubscriber<BudgetEventMessage>(Topics.BUDGET, 0, messageFactory.WildcardFilter);
            _budgetSubscriber.OnReceive += HandleBudgetRequests;
            _budgetCache = budgetCache;
            _log = logger;
            _budgetClient = client;
        }

        /// <summary>
        /// Handle events
        /// </summary>
        /// <param name="budgetEvent"></param>
        /// <returns></returns>
        async Task HandleBudgetRequests(BudgetEventMessage budgetEvent)
        {
            _log.LogInformation("Recieved budget event");
            var evt = budgetEvent.Body;
            try
            {
                // Only update our requests
                if (_ids.Remove(evt.CorrelationId))
                {
                    if (await _budgetCache.TryUpdateBudget(budgetEvent.Body.EntityId, budgetEvent.Body.Amount))
                        _log.LogInformation("Added {0} to {1}", budgetEvent.Body.Amount, budgetEvent.Body.EntityId);
                    else
                        _log.LogWarning("Failed to add {0} to {1}", budgetEvent.Body.Amount, budgetEvent.Body.EntityId);

                    _entities.Remove(evt.EntityId);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to process message");
            }
        }

        /// <inheritdoc/>
        public async Task GetAdditional(string id) => await GetAdditional(1, id);

        /// <inheritdoc/>
        public async Task GetAdditional(double amount, string id)
        {
            var correlationId = SequentialGuid.NextGuid();

            if (_entities.Add(id))
            {
                // Double check available here
                if (await GetRemaining(id) <= 0)
                {
                    _log.LogInformation("Requesting {0} for {1} ({2})", amount, id, correlationId);
                    _ids.Add(correlationId);
                    if (!await _budgetClient.RequestBudget(id, amount, correlationId))
                    {
                        _ids.Remove(correlationId);
                        _entities.Remove(id);
                    }
                }
                else
                    _entities.Remove(id);
            }
        }

        /// <inheritdoc/>
        public async Task<double> GetRemaining(string id) => await _budgetCache.GetBudget(id);

        /// <inheritdoc/>
        public async Task<bool> IsExhausted(string id) => await GetRemaining(id) <= 0;

        /// <inheritdoc/>
        public async Task<bool> TrySpend(double amount, string id) => await _budgetCache.TryUpdateBudget(id, amount);
    }
}