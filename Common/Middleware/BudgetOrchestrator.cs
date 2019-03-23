using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Handle Campaign API management
    /// </summary>
    public class BudgetOrchestrator
    {
        readonly IStorageManager _storageManager;
        readonly ISerializationContext _serializationContext;
        readonly ILogger<BudgetOrchestrator> _logger;
        readonly IStorageRepository<Campaign> _campaignRepository;
        readonly IMessageFactory _messageFactory;
        static readonly MemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15) });
        IBudgetCache _budgetCache;
        readonly IMessagePublisher _budgetPublisher;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <param name="budgetCache"></param>
        public BudgetOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<BudgetOrchestrator> logger, IBudgetCache budgetCache)
        {
            _storageManager = storageManager;
            _campaignRepository = storageManager.GetRepository<Campaign>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;
            _budgetCache = budgetCache;
            _budgetPublisher = _messageFactory.CreatePublisher(Topics.BUDGET);
        }

        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            _logger.LogInformation("Reading request");
            var request = await _serializationContext.ReadAs<BudgetRequest>(httpContext);

            if (request != null)
            {
                _logger.LogInformation("Request : {0}", request.CorrelationId);

                // Validate
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Unknown,
                    EntityId = request.EntityId,
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "post":

                        // Add a dollar if they've been good...
                        double? entry = (double?)_memcache.Get(request.EntityId);
                        _logger.LogInformation("Budget for {0} : {1}", request.EntityId, entry ?? 0d);
                        if (entry == null)
                        {
                            var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                            entry = await _budgetCache.TryUpdateBudget(request.EntityId, 1.0);
                            if (entry != double.NaN)
                            {
                                _logger.LogInformation("Budget for {0} : {1}", request.EntityId, entry ?? 0d);
                                msg.Body = new BudgetEvent { EntityId = request.EntityId, Exhausted = entry <= 0 };
                                await _budgetPublisher.TryPublish(msg);
                                entry = 1d;
                                _memcache.Set(request.EntityId, entry,
                                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300) });
                            }
                            else
                                _logger.LogWarning("Failed to update budget for {0}", request.EntityId);
                        }
                        else if (entry < 5)
                        {
                            var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                            entry = await _budgetCache.TryUpdateBudget(request.EntityId, 1.0);
                            if (entry != double.NaN)
                            {
                                _logger.LogInformation("Budget for {0} : {1}", request.EntityId, entry ?? 0d);
                                msg.Body = new BudgetEvent { EntityId = request.EntityId, Exhausted = entry <= 0 };
                                await _budgetPublisher.TryPublish(msg);
                                entry += 1;
                                _memcache.Set(request.EntityId, entry);
                            }
                            else
                                _logger.LogWarning("Failed to update budget for {0}", request.EntityId);
                        }
                        else
                            _logger.LogInformation("Budget rate exceeded for {0}", request.EntityId);

                        httpContext.Response.StatusCode = 202;
                        break;
                    case "get":
                    case "put":
                    case "patch":
                    case "delete":
                        httpContext.Response.StatusCode = 405;
                        break;
                }

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = "campaign";
                    await _messageFactory.CreatePublisher(Topics.BIDDING).TryPublish(msg);

                    var sync = _messageFactory.CreateMessage<LucentMessage<BudgetRequest>>();
                    sync.Body = request;
                    sync.Route = "campaign";
                    await _messageFactory.CreatePublisher(Topics.ENTITIES).TryBroadcast(msg);
                }
            }
        }
    }
}