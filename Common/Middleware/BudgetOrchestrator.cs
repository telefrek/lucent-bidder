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
using Prometheus;

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
        readonly IMessageFactory _messageFactory;
        readonly IAerospikeCache _aerospikeCache;
        IBudgetCache _budgetCache;
        readonly IMessagePublisher _budgetPublisher;
        readonly SemaphoreSlim _budgetLock = new SemaphoreSlim(1);
        StorageCache _storageCache;

        static Gauge _budgetValue = Metrics.CreateGauge("current_budget", "The current budget for an item", new GaugeConfiguration
        {
            LabelNames = new string[] { "entity", "type" }
        });

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <param name="budgetCache"></param>
        /// <param name="aerospikeCache"></param>
        /// <param name="storageCache"></param>
        public BudgetOrchestrator(RequestDelegate next, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<BudgetOrchestrator> logger, IBudgetCache budgetCache, IAerospikeCache aerospikeCache, StorageCache storageCache)
        {
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;
            _budgetCache = budgetCache;
            _budgetPublisher = _messageFactory.CreatePublisher(Topics.BUDGET);
            _aerospikeCache = aerospikeCache;
            _storageCache = storageCache;
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
                        BidCounters.BudgetRequests.WithLabels("recieved").Inc();

                        await _budgetLock.WaitAsync();

                        var schedule = (BudgetSchedule)null;
                        var entityName = (string)null;

                        switch (request.EntityType)
                        {
                            case EntityType.Campaign:
                                var camp = await _storageCache.Get<Campaign>(new StringStorageKey(request.EntityId), true);
                                if (camp != null)
                                {
                                    schedule = camp.BudgetSchedule;
                                    entityName = camp.Name;
                                }
                                break;
                            case EntityType.Exchange:
                                var exchange = await _storageCache.Get<Exchange>(new GuidStorageKey(Guid.Parse(request.EntityId)), true);
                                if (exchange != null)
                                {
                                    schedule = exchange.BudgetSchedule;
                                    entityName = exchange.Name;
                                }
                                break;
                            default:
                                break;
                        }

                        if (schedule == null)
                        {
                            httpContext.Response.StatusCode = 400;
                            return;
                        }


                        var status = await _budgetCache.TryGetRemaining(request.EntityId);
                        if (status.Successful)
                        {
                            var elapsed = DateTime.UtcNow.Subtract(status.LastUpdate).TotalMinutes;
                            _logger.LogInformation("Budget Elapsed: ({0} - {1} = {2}) [{3},{4},{5},{6}]", DateTime.UtcNow, status.LastUpdate, elapsed, status.Spend, status.TotalSpend, schedule.HourlyCap, schedule.DailyCap);
                            var allocation = new BudgetAllocation
                            {
                                Key = request.EntityId,
                                ResetSpend = DateTime.UtcNow.Hour > status.LastHourlyRollover.Hour,
                                ResetDaily = DateTime.UtcNow.Day > status.LastDailyRollover.Day
                            };

                            // Reset everything
                            if (allocation.ResetDaily)
                            {
                                allocation.ResetSpend = true;
                                var rem = schedule.HourlyCap;

                                if (schedule.ScheduleType == ScheduleType.Even)
                                    rem = Math.Min(schedule.HourlyCap / 12, rem);

                                allocation.Amount = rem;
                            }
                            else if (schedule.DailyCap > status.TotalSpend)
                            {
                                // Check for hourly rollover
                                if (allocation.ResetSpend)
                                {
                                    var rem = Math.Min(schedule.HourlyCap, schedule.DailyCap - status.TotalSpend);

                                    if (schedule.ScheduleType == ScheduleType.Even)
                                        rem = elapsed >= 5 ? Math.Min(schedule.HourlyCap / 12, rem) : 0;

                                    allocation.Amount = rem;
                                }
                                else
                                {
                                    // Min of remaining hourly or daily for edge cases
                                    var rem = Math.Min(schedule.HourlyCap - status.Spend, schedule.DailyCap - status.TotalSpend);

                                    if (schedule.ScheduleType == ScheduleType.Even)
                                        rem = elapsed >= 5 ? Math.Min(schedule.HourlyCap / 12, rem) : 0;

                                    allocation.Amount = rem;
                                }
                            }

                            if (allocation.Amount > 0)
                                status = await _budgetCache.TryUpdateBudget(allocation);
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