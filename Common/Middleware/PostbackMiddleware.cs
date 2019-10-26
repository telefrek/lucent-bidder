using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var status = StatusCodes.Status404NotFound;
            var suppress = false;
            var op = BidOperation.Unknown;

            if (query.ContainsKey(QueryParameters.LUCENT_REDIRECT_PARAMETER))
            {
                suppress = true;
                BidCounters.Redirects.Inc();
                context.Response.Redirect(Encoding.UTF8.GetString(Convert.FromBase64String(query[QueryParameters.LUCENT_REDIRECT_PARAMETER].First().SafeBase64Decode())));
            }

            try
            {
                BidContext bidContext = new BidContext();

                if (query.ContainsKey(QueryParameters.LUCENT_BID_CONTEXT_PARAMETER))
                    bidContext = BidContext.Parse(query[QueryParameters.LUCENT_BID_CONTEXT_PARAMETER]);

                var campaign = (await _storageCache.Get<Campaign>(new StringStorageKey(bidContext.CampaignId.ToString())));

                BidCounters.Postbacks.WithLabels(bidContext.Operation.ToString(), campaign.Name).Inc();

                var stats = CampaignStats.Get(campaign.Id);

                var metadata = new Dictionary<string, object>();
                op = bidContext.Operation;

                switch (bidContext.Operation)
                {
                    case BidOperation.Loss:
                        status = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Clicked:
                    case BidOperation.Win:
                        if (bidContext.Operation == BidOperation.Clicked)
                        {
                            BidCounters.CampaignClicks.WithLabels(campaign.Name).Inc();
                        }

                        var cpm = 0d;
                        if (!TryGetCPM(context, out cpm))
                            cpm = bidContext.CPM;
                        // Update the exchange and campaign amounts
                        // TODO: handle errors
                        var acpm = Math.Round(cpm / 1000d, 5);
                        stats.CPM.Inc(acpm);
                        if (bidContext.Operation == BidOperation.Win)
                            stats.Wins.Inc(1);
                        var entry = new BidEntry { BidContext = bidContext.ToString(), RequestId = bidContext.RequestId, Cost = acpm };
                        var response = await _bidCache.getEntryAsync(bidContext.RequestId);
                        if (response == null)
                        {
                            _log.LogWarning("No bid response found for context : {0} ({1})", bidContext.ToString(), bidContext.Operation);
                            status = StatusCodes.Status404NotFound;
                            break;
                        }

                        foreach (var key in response.Keys)
                            metadata.Add(key, response[key]);

                        if (bidContext.Operation == BidOperation.Win)
                        {
                            BidCounters.CampaignCPM.WithLabels(campaign.Name).Inc(cpm);
                            BidCounters.CampaignSpend.WithLabels(campaign.Name).Inc(acpm);
                            BidCounters.CampaignWins.WithLabels(campaign.Name).Inc();

                            // Check for shutdown
                            var exchangeId = bidContext.ExchangeId.ToString();
                            var res = await _budgetCache.TryUpdateSpend(exchangeId, acpm);
                            if (res.Successful && res.Remaining <= 0 && _memcache.Get(exchangeId) == null)
                            {
                                var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                                msg.Body = new BudgetEvent { EntityId = bidContext.ExchangeId.ToString(), Exhausted = true };
                                if (await _budgetPublisher.TryPublish(msg))
                                    _memcache.Set(exchangeId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });
                            }

                            var campaignId = bidContext.CampaignId.ToString();
                            res = await _budgetCache.TryUpdateSpend(campaignId, acpm);

                            if (res.Successful && res.Remaining <= 0 && _memcache.Get(campaignId) == null)
                            {
                                var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                                msg.Body = new BudgetEvent { EntityId = bidContext.CampaignId.ToString(), Exhausted = true };
                                if (await _budgetPublisher.TryPublish(msg))
                                    _memcache.Set(campaignId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });
                            }
                        }
                        else
                        {
                            entry.Cost = 0;
                        }

                        metadata.Add(bidContext.Operation.ToString().ToLower(), 1);
                        await _ledger.TryRecordEntry(bidContext.ExchangeId.ToString(), entry, metadata);
                        await _ledger.TryRecordEntry(bidContext.CampaignId.ToString(), entry, metadata);

                        status = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Impression:
                        suppress = true;
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        BidCounters.CampaignImpressions.WithLabels(campaign.Name).Inc();
                        context.Response.Headers.Add("Content-Type", "image/png");
                        context.Response.Headers.ContentLength = PIXEL_BYTES.Length;
                        await context.Response.Body.WriteAsync(PIXEL_BYTES, 0, PIXEL_BYTES.Length);
                        await context.Response.Body.FlushAsync();
                        return;
                    case BidOperation.Action:
                        if (campaign != null)
                        {
                            var payout = 0d;
                            if (context.Request.Query.ContainsKey("payout"))
                                Double.TryParse(context.Request.Query["payout"].First(), out payout);
                            _log.LogInformation("Postback {0} for {1}", payout, campaign.Id);
                            metadata.Add("postback", 1);
                            stats.Conversions.Inc(1);
                            BidCounters.CampaignConversions.WithLabels(campaign.Name).Inc();
                            BidCounters.CampaignRevenue.WithLabels(campaign.Name).Inc(payout);
                            await _ledger.TryRecordEntry(campaign.Id, new BidEntry { RequestId = bidContext.RequestId, Cost = payout, IsRevenue = true }, metadata);
                            if (payout > 0)
                                LocalBudget.Get(campaign.Id).ActionLimit.Inc(1);
                        }
                        else
                            _log.LogWarning("Campaign not found for action processing");
                        status = StatusCodes.Status200OK;
                        break;
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to handle postback");
                status = StatusCodes.Status400BadRequest;
            }
            finally
            {
                if (!suppress)
                {
                    try
                    {
                        context.Response.StatusCode = status;
                    }
                    catch (Exception e)
                    {
                        _log.LogError(e, "failed to set status for non-suppressed item during {0}", op.ToString());
                    }
                }
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