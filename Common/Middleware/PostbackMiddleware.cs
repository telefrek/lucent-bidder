using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Prometheus;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for controlling postback notifications
    /// </summary>
    public class PostbackMiddleware
    {
        ILogger<PostbackMiddleware> _log;

        static readonly IMemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(5) });

        ISerializationContext _serializationContext;
        IStorageManager _storageManager;
        IBidCache _bidCache;
        IBidLedger _ledger;
        IMessageFactory _messageFactory;
        IMessagePublisher _budgetPublisher;
        IStorageRepository<Campaign> _campaignRepo;
        StorageCache _storageCache;
        IBudgetCache _budgetCache;
        static readonly byte[] PIXEL_BYTES = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="log"></param>
        /// <param name="factory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="bidderCache"></param>
        /// <param name="budgetLedger"></param>
        /// <param name="messageFactory"></param>
        /// <param name="storageCache"></param>
        /// <param name="budgetCache"></param>
        public PostbackMiddleware(RequestDelegate next, ILogger<PostbackMiddleware> log, IMessageFactory factory, ISerializationContext serializationContext, IBidCache bidderCache, IBidLedger budgetLedger, IMessageFactory messageFactory, StorageCache storageCache, IBudgetCache budgetCache)
        {
            _log = log;
            _serializationContext = serializationContext;
            _bidCache = bidderCache;
            _ledger = budgetLedger;
            _messageFactory = messageFactory;
            _budgetPublisher = _messageFactory.CreatePublisher(Topics.BUDGET);
            _storageCache = storageCache;
            _budgetCache = budgetCache;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check type of postback
            var query = context.Request.Query;

            try
            {
                BidContext bidContext = new BidContext();

                if (query.ContainsKey(QueryParameters.LUCENT_REDIRECT_PARAMETER))
                    context.Response.Redirect(query[QueryParameters.LUCENT_REDIRECT_PARAMETER].First().SafeBase64Decode());

                if (query.ContainsKey(QueryParameters.LUCENT_BID_CONTEXT_PARAMETER))
                    bidContext = BidContext.Parse(query[QueryParameters.LUCENT_BID_CONTEXT_PARAMETER]);

                var campaign = (await _storageCache.Get<Campaign>(new StringStorageKey(bidContext.CampaignId.ToString())));

                var stats = CampaignStats.Get(campaign.Id);

                switch (bidContext.Operation)
                {
                    case BidOperation.Clicked:
                        // Start the rest of the tracking async
                        break;
                    case BidOperation.Loss:
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Win:
                        var cpm = 0d;
                        if (!TryGetCPM(context, out cpm))
                            cpm = bidContext.CPM;
                        // Update the exchange and campaign amounts
                        // TODO: handle errors
                        var acpm = Math.Round(cpm / 1000d, 4);
                        stats.CPM.Inc(acpm);
                        stats.Wins.Inc(1);
                        var entry = new BidEntry { BidContext = bidContext.ToString(), RequestId = bidContext.RequestId, Cost = acpm };
                        var response = await _bidCache.getEntryAsync(bidContext.RequestId);
                        if (response == null)
                        {
                            _log.LogWarning("No bid response found for context : {0}", bidContext.ToString());
                            context.Response.StatusCode = StatusCodes.Status404NotFound;
                            return;
                        }

                        entry.Bid = response.Bids.SelectMany(sb => sb.Bids).First(b => b.Id == bidContext.BidId.ToString());
                        if (entry.Bid == null)
                        {
                            _log.LogWarning("No bid found for id : {0}", bidContext.BidId);
                            context.Response.StatusCode = StatusCodes.Status404NotFound;
                            return;
                        }

                        BidCounters.CampaignSpend.WithLabels(campaign.Name).Inc(acpm);
                        BidCounters.CampaignWins.WithLabels(campaign.Name).Inc();

                        await _ledger.TryRecordEntry(bidContext.ExchangeId.ToString(), entry);
                        await _ledger.TryRecordEntry(bidContext.CampaignId.ToString(), entry);

                        // Check for shutdown
                        var exchangeId = bidContext.ExchangeId.ToString();
                        var status = await _budgetCache.TryUpdateSpend(exchangeId, acpm);
                        if (status.Successful && status.Remaining <= 0 && _memcache.Get(exchangeId) == null)
                        {
                            var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                            msg.Body = new BudgetEvent { EntityId = bidContext.ExchangeId.ToString(), Exhausted = true };
                            if (await _budgetPublisher.TryPublish(msg))
                                _memcache.Set(exchangeId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });
                        }

                        var campaignId = bidContext.CampaignId.ToString();
                        status = await _budgetCache.TryUpdateSpend(campaignId, acpm);

                        if (status.Successful && status.Remaining <= 0 && _memcache.Get(campaignId) == null)
                        {
                            var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                            msg.Body = new BudgetEvent { EntityId = bidContext.CampaignId.ToString(), Exhausted = true };
                            if (await _budgetPublisher.TryPublish(msg))
                                _memcache.Set(campaignId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });
                        }

                        context.Response.StatusCode = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Impression:
                        BidCounters.CampaignImpressions.WithLabels(campaign.Name).Inc();
                        context.Response.Headers.Add("Content-Type", "image/png");
                        context.Response.Headers.ContentLength = PIXEL_BYTES.Length;
                        _log.LogInformation("Writing {0} bytes", PIXEL_BYTES.Length);
                        await context.Response.Body.WriteAsync(PIXEL_BYTES, 0, PIXEL_BYTES.Length);
                        await context.Response.Body.FlushAsync();
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Action:
                        var action = "";
                        if (campaign != null && TryGetAction(context, out action))
                        {
                            if (campaign.Actions != null)
                            {
                                var postbackAction = campaign.Actions.FirstOrDefault(pb => pb.Name.Equals(action, StringComparison.InvariantCultureIgnoreCase));
                                if (postbackAction != null)
                                {
                                    stats.Conversions.Inc(1);
                                    BidCounters.CampaignConversions.WithLabels(campaign.Name).Inc();
                                    BidCounters.CampaignRevenue.WithLabels(campaign.Name).Inc(postbackAction.Payout);
                                    await _ledger.TryRecordEntry(campaign.Id, new BidEntry { RequestId = bidContext.RequestId, Cost = postbackAction.Payout, IsRevenue = true });
                                    LocalBudget.Get(campaign.Id).ActionLimit.Inc(1);
                                }
                                else
                                    _log.LogWarning("Action {0} is not on campaign", action);
                            }
                            else
                            {
                                _log.LogWarning("No actions defined for campaign");
                            }
                        }
                        else
                            _log.LogWarning("Campaign not found for action processing");
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        break;
                    default:
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        break;
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to handle postback");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        /// <summary>
        /// Attempt to extract the cpm value from the context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cpm"></param>
        /// <returns>True if it exists</returns>
        bool TryGetCPM(HttpContext context, out double cpm)
        {
            cpm = 0;

            StringValues sv;
            if (context.Request.Query.TryGetValue("cpm", out sv))
                return double.TryParse(sv, out cpm);

            return false;
        }

        /// <summary>
        /// Try to get the action from the request 
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <param name="action">The action output</param>
        /// <returns>True if it exists</returns>
        bool TryGetAction(HttpContext context, out string action)
        {
            action = null;
            StringValues sv;
            if (context.Request.Query.TryGetValue(QueryParameters.LUCENT_BID_ACTION_PARAMETER, out sv))
            {
                action = sv;
                return true;
            }

            return false;
        }
    }
}