using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        public SimpleBudgetManager(IMessageFactory messageFactory, ILogger<SimpleBudgetManager> logger, IBudgetClient client)
        {
            _budgetSubscriber = messageFactory.CreateSubscriber<BudgetEventMessage>(Topics.BUDGET, 0, messageFactory.WildcardFilter);
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
            var evt = budgetEvent.Body;
            try
            {
                await OnStatusChanged(evt);
            }
            catch (Exception e)
            {
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

            if (await _budgetClient.RequestBudget(entityId))
                // only request additional every minute
                _memcache.Set(entityId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });

        }
    }
}