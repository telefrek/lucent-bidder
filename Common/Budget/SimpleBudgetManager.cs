using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.Caching;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// This is terrible
    /// </summary>
    public class SimpleBudgetManager : IBudgetManager
    {
        static readonly MemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15) });

        HashSet<string> _entities = new HashSet<string>();
        HashSet<Guid> _ids = new HashSet<Guid>();
        IMessageSubscriber<BudgetEventMessage> _budgetSubscriber;
        ILogger _log;
        IBudgetClient _budgetClient;
        readonly SemaphoreSlim _budgetSem = new SemaphoreSlim(1);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        public SimpleBudgetManager(IMessageFactory messageFactory, ILogger<SimpleBudgetManager> logger, IBudgetClient client)
        {
            _budgetSubscriber = messageFactory.CreateSubscriber<BudgetEventMessage>(Topics.BUDGET, messageFactory.WildcardFilter);
            _budgetSubscriber.OnReceive += HandleBudgetRequests;
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
            BidCounters.BudgetRequests.WithLabels("response").Inc();
            var evt = budgetEvent.Body;
            try
            {
                await OnStatusChanged(evt);
            }
            catch (Exception e)
            {
                BidCounters.BudgetRequests.WithLabels("error").Inc();
                _log.LogError(e, "Failed to process message");
            }
        }

        /// <inheritdoc/>
        public Func<BudgetEvent, Task> OnStatusChanged { get; set; }

        /// <inheritdoc/>
        public async Task RequestAdditional(string entityId)
        {
            if (_memcache.Get(entityId) != null)
                return;

            await _budgetSem.WaitAsync();
            try
            {
                if (_memcache.Get(entityId) != null)
                    return;

                _log.LogInformation("Requesting budget for {0}", entityId);

                _memcache.Set(entityId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });

                BidCounters.BudgetRequests.WithLabels("request").Inc();
                if (!await _budgetClient.RequestBudget(entityId))
                    _memcache.Remove(entityId);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed t get additional budget for: {0}", entityId);
                _memcache.Remove(entityId);
            }
            finally
            {
                _budgetSem.Release();
            }

        }
    }
}