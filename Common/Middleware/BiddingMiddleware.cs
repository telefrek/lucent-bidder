using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Lucent.Common.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Prometheus;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for controlling bids
    /// </summary>
    public class BiddingMiddleware
    {
        ILogger<BiddingMiddleware> _log;
        ISerializationContext _serializationContext;
        IMessageFactory _messageFactory;
        IExchangeRegistry _exchangeRegistry;
        IStorageManager _storageManager;
        IBidCache _bidCache;
        UpdatingCollection<BidderFilter> _bidFiltersCollection;
        List<Func<BidRequest, bool>> _bidFilters;
        Histogram _serializerTiming = Metrics.CreateHistogram("serializer_latency", "Latency for each bidder call", new HistogramConfiguration
        {
            LabelNames = new string[] { "protocol", "direction" },
            Buckets = new double[] { 0.001, 0.002, 0.005, 0.007, 0.01, 0.015 },
        });
        StorageCache _storageCache;



        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="exchangeRegistry"></param>
        /// <param name="storageManager"></param>
        /// <param name="bidCache"></param>
        public BiddingMiddleware(RequestDelegate next, ILogger<BiddingMiddleware> logger, IMessageFactory messageFactory, ISerializationContext serializationContext, IExchangeRegistry exchangeRegistry, IStorageManager storageManager, IBidCache bidCache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _exchangeRegistry = exchangeRegistry;
            _storageManager = storageManager;
            _bidFiltersCollection = new UpdatingCollection<BidderFilter>(messageFactory, storageManager, logger, EntityType.BidderFilter);
            _bidFiltersCollection.OnUpdate = UpdateBidFilters;
            _bidFilters = _bidFiltersCollection.Entities.Where(f => f.BidFilter != null).Select(f => f.BidFilter.GenerateCode()).ToList();
            _messageFactory = messageFactory;
            _bidCache = bidCache;
            _storageCache = new StorageCache(serializationContext, storageManager);
        }

        /// <summary>
        /// Update bid filterss
        /// </summary>
        /// <returns></returns>
        Task UpdateBidFilters()
        {
            _bidFilters = _bidFiltersCollection.Entities.Where(f => f.BidFilter != null).Select(f => f.BidFilter.GenerateCode()).ToList();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="httpContext">The current http context</param>
        /// <param name="userManager">The user manager instance</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext httpContext, IUserManager userManager)
        {
            try
            {
                var request = await _serializationContext.ReadAs<BidRequest>(httpContext);

                if (request == null)
                {
                    BidCounters.NoBidReason.WithLabels("deserialization_failure").Inc();
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
                else if (!_bidFilters.Any(f => f.Invoke(request)))
                {
                    // Validate we can find a matching exchange
                    var exchange = _exchangeRegistry.GetExchange(httpContext);
                    if (exchange == null)
                    {
                        BidCounters.NoBidReason.WithLabels("no_exchange").Inc();
                        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                        return;
                    }
                    httpContext.Items.Add("exchange", exchange);

                    // Get the user features
                    var userFeatures = await userManager.GetFeaturesAsync(request);
                    httpContext.Items.Add("userFeatures", userFeatures);

                    var response = await exchange.Bid(request, httpContext);

                    if (response != null)
                    {
                        response.Bids = response.Bids ?? new SeatBid[0];
                        if (response.Bids.Length > 0)
                        {
                            // Incremenb bid counters
                            foreach (var c in response.Bids.SelectMany(b => b.Bids).Select(b => b.CampaignId).Distinct().Select(c=>_storageCache.Get<Campaign>(new StringStorageKey(c))))
                                await c.ContinueWith(t=>BidCounters.CampaignBids.WithLabels(t.Result.Name).Inc());

                            httpContext.Response.StatusCode = StatusCodes.Status200OK;
                            await _serializationContext.WriteTo(httpContext, response);
                            await _bidCache.saveEntries(response);
                        }
                        else
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                            BidCounters.NoBidReason.WithLabels("no_campaign_bids").Inc();
                        }
                    }
                    else
                    {
                        BidCounters.NoBidReason.WithLabels("no_response").Inc();
                        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                    }
                }
                else
                {
                    BidCounters.NoBidReason.WithLabels("bid_filtered").Inc();
                    httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to bid");
                BidCounters.NoBidReason.WithLabels("exception").Inc();
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            }
        }
    }
}