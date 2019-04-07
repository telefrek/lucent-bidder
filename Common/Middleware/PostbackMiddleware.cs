using System;
using System.Collections.Generic;
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

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for controlling postback notifications
    /// </summary>
    public class PostbackMiddleware
    {
        ILogger<PostbackMiddleware> _log;

        static readonly MemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(5) });

        ISerializationContext _serializationContext;
        IStorageManager _storageManager;
        IBidCache _bidCache;
        IBidLedger _ledger;
        IMessageFactory _messageFactory;
        IMessagePublisher _budgetPublisher;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="log"></param>
        /// <param name="factory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="storageManager"></param>
        /// <param name="bidderCache"></param>
        /// <param name="budgetLedger"></param>
        /// <param name="messageFactory"></param>
        public PostbackMiddleware(RequestDelegate next, ILogger<PostbackMiddleware> log, IMessageFactory factory, ISerializationContext serializationContext, IStorageManager storageManager, IBidCache bidderCache, IBidLedger budgetLedger, IMessageFactory messageFactory)
        {
            _log = log;
            _serializationContext = serializationContext;
            _storageManager = storageManager;
            _bidCache = bidderCache;
            _ledger = budgetLedger;
            _messageFactory = messageFactory;
            _budgetPublisher = _messageFactory.CreatePublisher(Topics.BUDGET);
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
                        if (TryGetCPM(context, out cpm))
                        {
                            // Update the exchange and campaign amounts
                            // TODO: handle errors
                            var acpm = cpm / 1000d;
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

                            await _ledger.TryRecordEntry(bidContext.ExchangeId.ToString(), entry);
                            await _ledger.TryRecordEntry(bidContext.CampaignId.ToString(), entry);

                            // Check for shutdown
                            var exchangeId = bidContext.ExchangeId.ToString();
                            var exchgBudget = LocalBudget.Get(exchangeId);
                            if (exchgBudget.Update(-acpm) <= 0 && _memcache.Get(exchangeId) == null)
                            {
                                var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                                msg.Body = new BudgetEvent { EntityId = bidContext.CampaignId.ToString(), Exhausted = true };
                                if (await _budgetPublisher.TryPublish(msg))
                                    _memcache.Set(exchangeId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });

                            }

                            var campaignId = bidContext.CampaignId.ToString();
                            var campaignBudget = LocalBudget.Get(campaignId);

                            if (campaignBudget.Update(-acpm) <= 0 && _memcache.Get(campaignId) == null)
                            {
                                var msg = _messageFactory.CreateMessage<BudgetEventMessage>();
                                msg.Body = new BudgetEvent { EntityId = bidContext.CampaignId.ToString(), Exhausted = true };
                                if (await _budgetPublisher.TryPublish(msg))
                                    _memcache.Set(campaignId, new object(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });
                            }
                        }
                        else
                        {
                            _log.LogWarning("No pricing on {0}", context.Request.QueryString);
                        }

                        context.Response.StatusCode = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Impression:
                        await Task.Delay(10);
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        break;
                    case BidOperation.Action:
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
        /// <returns></returns>
        bool TryGetCPM(HttpContext context, out double cpm)
        {
            cpm = 0;

            StringValues sv;
            if (context.Request.Query.TryGetValue("cpm", out sv))
                return double.TryParse(sv, out cpm);

            return false;
        }
    }
}