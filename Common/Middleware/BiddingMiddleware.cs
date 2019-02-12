using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
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
        IBudgetCache _bidCache;
        UpdatingCollection<BidderFilter> _bidFiltersCollection;
        List<Func<BidRequest, bool>> _bidFilters;
        Histogram _serializerTiming = Metrics.CreateHistogram("serializer_latency", "Latency for each bidder call", new HistogramConfiguration
        {
            LabelNames = new string[] { "protocol", "direction" },
            Buckets = new double[] { 0.001, 0.002, 0.005, 0.007, 0.01, 0.015 },
        });

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="exchangeRegistry"></param>
        /// <param name="storageManager"></param>
        /// <param name="bidderCache"></param>
        public BiddingMiddleware(RequestDelegate next, ILogger<BiddingMiddleware> logger, IMessageFactory messageFactory, ISerializationContext serializationContext, IExchangeRegistry exchangeRegistry, IStorageManager storageManager, IBudgetCache bidderCache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _exchangeRegistry = exchangeRegistry;
            _storageManager = storageManager;
            _bidFiltersCollection = new UpdatingCollection<BidderFilter>(messageFactory, storageManager, EntityType.BidderFilter);
            _bidFiltersCollection.OnUpdate = UpdateBidFilters;
            _bidFilters = _bidFiltersCollection.Entities.Where(f => f.BidFilter != null).Select(f => f.BidFilter.GenerateCode()).ToList();
            _messageFactory = messageFactory;
            _bidCache = bidderCache;
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
        /// 
        /// </summary>
        /// <param name="entityEvent"></param>
        /// <returns></returns>
        public async Task TrackEntities(EntityEventMessage entityEvent)
        {
            switch (entityEvent.Body.EventType)
            {
                case EventType.EntityAdd:
                case EventType.EntityUpdate:
                    switch (entityEvent.Body.EntityType)
                    {
                        case EntityType.Exchange:
                            var exchange = await _storageManager.GetRepository<Exchange>().Get(new GuidStorageKey(Guid.Parse(entityEvent.Body.EntityId)));
                            if (exchange != null)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    await _serializationContext.WriteTo(exchange, ms, true, SerializationFormat.JSON);
                                    _log.LogInformation("Received Exchange:\n{0}", Encoding.UTF8.GetString(ms.ToArray()));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EventType.EntityDelete:
                    break;
                default:
                    _log.LogWarning("Invalid event type: {0}", entityEvent.Body.EventType);
                    break;
            }
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="httpContext">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {

                var request = await _serializationContext.ReadAs<BidRequest>(httpContext);

                if (request != null && !_bidFilters.Any(f => f.Invoke(request)))
                {
                    // Validate we can find a matching exchange
                    var exchange = _exchangeRegistry.GetExchange(httpContext);
                    if (exchange == null)
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                        return;
                    }

                    httpContext.Items.Add("exchange", exchange);
                    var response = await exchange.Bid(request, httpContext);

                    if (response != null && (response.Bids ?? new SeatBid[0]).Length > 0)
                    {
                        foreach (var seat in response.Bids)
                            foreach (var bid in seat.Bids)
                                try
                                {
                                    await _bidCache.TryStore(bid, bid.Id, TimeSpan.FromMinutes(5));
                                }
                                catch
                                {

                                }

                        httpContext.Response.StatusCode = StatusCodes.Status200OK;
                        await _serializationContext.WriteTo(httpContext, response);
                    }
                    else
                        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                }
                else
                    httpContext.Response.StatusCode = request == null ? StatusCodes.Status400BadRequest : StatusCodes.Status204NoContent;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to bid");
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            }
        }
    }
}