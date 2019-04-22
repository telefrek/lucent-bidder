using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Bidding;
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
        readonly IStorageRepository<Exchange> _exchangeRepository;
        readonly IMessageFactory _messageFactory;
        readonly IAerospikeCache _aerospikeCache;
        IBudgetCache _budgetCache;
        readonly IMessagePublisher _budgetPublisher;
        readonly SemaphoreSlim _budgetLock = new SemaphoreSlim(1);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <param name="budgetCache"></param>
        /// <param name="aerospikeCache"></param>
        public BudgetOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<BudgetOrchestrator> logger, IBudgetCache budgetCache, IAerospikeCache aerospikeCache)
        {
            _storageManager = storageManager;
            _campaignRepository = storageManager.GetRepository<Campaign>();
            _exchangeRepository = storageManager.GetRepository<Exchange>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;
            _budgetCache = budgetCache;
            _budgetPublisher = _messageFactory.CreatePublisher(Topics.BUDGET);
            _aerospikeCache = aerospikeCache;
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

                        await _budgetLock.WaitAsync();

                        var schedule = (BudgetSchedule)null;

                        switch (request.EntityType)
                        {
                            case EntityType.Campaign:
                                var camp = await _campaignRepository.Get(new StringStorageKey(request.EntityId));
                                if (camp != null)
                                    schedule = camp.BudgetSchedule;
                                break;
                            case EntityType.Exchange:
                                var exchange = await _exchangeRepository.Get(new GuidStorageKey(Guid.Parse(request.EntityId)));
                                if (exchange != null)
                                    schedule = exchange.BudgetSchedule;
                                break;
                            default:
                                break;
                        }

                        if (schedule == null)
                        {
                            httpContext.Response.StatusCode = 400;
                            return;
                        }

                        var ttl = 5;
                        var amount = 0d;

                        switch (schedule.ScheduleType)
                        {
                            case ScheduleType.Aggressive:
                                ttl = 60 - DateTime.Now.Minute;
                                amount = schedule.HourlyCap;
                                break;
                            case ScheduleType.Even:
                                amount = schedule.HourlyCap / 12; // 5 minutes per segment
                                break;
                            default:
                                httpContext.Response.StatusCode = 400;
                                return;
                        }

                        if (amount > 0)
                        {

                            // Add a dollar if they've been good...
                            BidCounters.BudgetRequests.WithLabels("recieved").Inc();

                            if (await _aerospikeCache.TryUpdateBudget(request.EntityId, amount, schedule.HourlyCap, TimeSpan.FromMinutes(ttl)))
                                if (await _budgetCache.TryUpdateBudget(request.EntityId, amount) == double.NaN)
                                    await _aerospikeCache.TryUpdateBudget(request.EntityId, -amount, schedule.HourlyCap, TimeSpan.FromMinutes(ttl));
                        }

                        _budgetLock.Release();

                        var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                        msg.Body = new BudgetEvent { EntityId = request.EntityId };
                        await _budgetPublisher.TryPublish(msg);

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
                    msg.Route = request.EntityType.ToString().ToLowerInvariant();
                    await _messageFactory.CreatePublisher(Topics.BIDDING).TryPublish(msg);

                    var sync = _messageFactory.CreateMessage<LucentMessage<BudgetRequest>>();
                    sync.Body = request;
                    msg.Route = request.EntityType.ToString().ToLowerInvariant();
                    await _messageFactory.CreatePublisher(Topics.ENTITIES).TryBroadcast(msg);
                }
            }
        }
    }
}