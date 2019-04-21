using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;
using Lucent.Common.Formatters;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lucent.Common.Budget;
using System.Threading;
using Prometheus;

namespace Lucent.Samples.SimpleExchange
{
    /// <summary>
    /// Implementation of a very basic exchange
    /// </summary>
    public class SimpleExchange : AdExchange
    {
        IBiddingManager _bidManager;
        ILogger<SimpleExchange> _log;
        MarkupGenerator _markup = new MarkupGenerator();

        Histogram _bidderLatency = Metrics.CreateHistogram("exchange_bidder_latency", "Exchange bidder latency", new HistogramConfiguration
        {
            Buckets = MetricBuckets.LOW_LATENCY_10_MS
        });

        /// <inheritdoc/>
        public override async Task Initialize(IServiceProvider provider)
        {
            _log = provider.GetRequiredService<ILogger<SimpleExchange>>();
            _bidManager = provider.GetRequiredService<IBiddingManager>();
            await _bidManager.Initialize(this);
        }

        public Bid ExtractBid(BidContext bidContext)
        {
            var bid = bidContext.Bid;

            bid.AdMarkup = _markup.GenerateMarkup(bidContext);

            return bid;
        }

        /// <inheritdoc/>
        public override async Task<BidResponse> Bid(BidRequest request, HttpContext httpContext)
        {
            using (var histo = _bidderLatency.CreateContext())
                if (_bidManager.Bidders.Count > 0 && await _bidManager.CanBid())
                {
                    var resp = new BidResponse
                    {
                        NoBidReason = NoBidReason.SuspectedNonHuman,
                        Id = request.Id,
                        CorrelationId = SequentialGuid.NextGuid().ToString(),
                    };

                    //var bids = _bidManager.Bidders.Select(b => b.BidAsync(request, httpContext));

                    var managers = _bidManager.Bidders.Shuffle();


                    var seats = new List<SeatBid>();

                    foreach (var mgr in managers)
                    {
                        var bids = await mgr.BidAsync(request, httpContext);
                        if (bids != null)
                        {
                            var c = mgr.Campaign;

                            var seat = new SeatBid
                            {
                                BuyerId = c.BuyerId,
                                Bids = bids.Select(b =>
                                {
                                    switch (b.Content.ContentType)
                                    {
                                        case ContentType.Video:
                                            b.Bid.AdMarkup = b.ToVast();
                                            break; ;
                                        case ContentType.Banner:
                                            b.Bid.AdMarkup = b.ToImageLinkMarkup(new Uri(httpContext.Request.Scheme + "://" + httpContext.Request.Host.Value));
                                            break;
                                    }
                                    return b.Bid;
                                }).ToArray()
                            };

                            if (seat.Bids.Length > 0)
                                seats.Add(seat);
                        }
                    }

                    if (seats.Count > 0)
                    {
                        resp.Bids = seats.ToArray();
                        return resp;
                    }
                }

            return null;
        }
    }
}