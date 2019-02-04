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
        ISerializationContext _serializationContext;
        IStorageManager _storageManager;
        IBudgetCache _bidCache;
        IBudgetLedger _ledger;

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
        public PostbackMiddleware(RequestDelegate next, ILogger<PostbackMiddleware> log, IMessageFactory factory, ISerializationContext serializationContext, IStorageManager storageManager, IBudgetCache bidderCache, IBudgetLedger budgetLedger)
        {
            _log = log;
            _serializationContext = serializationContext;
            _storageManager = storageManager;
            _bidCache = bidderCache;
            _ledger = budgetLedger;
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

                _log.LogInformation("Bid : {0} ({1})", bidContext.BidId, bidContext.Operation);

                var bid = await _bidCache.TryRetrieve<Bid>(bidContext.BidId.ToString());
                if (bid != null)
                {
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
                                _log.LogInformation("Updating budgets for win {0} ({1})", cpm, acpm);
                                await _ledger.TryRecordEntry(bidContext.ExchangeId.ToString(), bid, EntityType.Bid, acpm);
                                await _ledger.TryRecordEntry(bidContext.CampaignId.ToString(), bid, EntityType.Bid, acpm);
                                await _bidCache.TryUpdateBudget(bidContext.ExchangeId.ToString(), -acpm);
                                await _bidCache.TryUpdateBudget(bidContext.CampaignId.ToString(), -acpm);
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
                else
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
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