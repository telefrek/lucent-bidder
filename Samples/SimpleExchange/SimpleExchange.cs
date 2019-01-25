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

namespace Lucent.Samples.SimpleExchange
{
    /// <summary>
    /// Implementation of a very basic exchange
    /// </summary>
    public class SimpleExchange : AdExchange
    {
        IBiddingManager _bidManager;
        ILogger<SimpleExchange> _log;


        /// <inheritdoc/>
        public override Task Initialize(IServiceProvider provider)
        {
            _log = provider.GetRequiredService<ILogger<SimpleExchange>>();
            _bidManager = provider.GetRequiredService<IBiddingManager>();

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<BidResponse> Bid(BidRequest request, HttpContext httpContext)
        {
            if (await _bidManager.CanBid(ExchangeId.ToString()) && _bidManager.Bidders.Count > 0)
            {
                var resp = new BidResponse
                {
                    NoBidReason = NoBidReason.SuspectedNonHuman,
                    Id = request.Id,
                    CorrelationId = SequentialGuid.NextGuid().ToString(),
                };

                var bids = _bidManager.Bidders.Select(b => b.BidAsync(request, httpContext));

                // Probably make this not wait on everything...
                await Task.WhenAll(bids);
                var seats = new List<SeatBid>();

                foreach (var bid in bids.Where(b => b.Result.Length > 0).Select(b => b.Result))
                {
                    var c = bid.First().Campaign;

                    var seat = new SeatBid
                    {
                        BuyerId = c.BuyerId,
                        Bids = bid.Select(b => FormatBid(b, httpContext)).ToArray()
                    };

                    if (seat.Bids.Length > 0)
                        seats.Add(seat);
                }

                if (seats.Count > 0)
                {
                    resp.Bids = seats.ToArray();
                    return resp;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override Bid FormatBid(BidMatch match, HttpContext httpContext)
        {
            var bidContext = new BidContext
            {
                BidDate = DateTime.UtcNow,
                BidId = Guid.Parse(match.RawBid.Id),
                CampaignId = Guid.Parse(match.Campaign.Id),
                ExchangeId = ExchangeId,
                CPM = match.RawBid.CPM,
            };

            // Format and stash/attach markup
            switch (match.Content.ContentType)
            {
                case ContentType.Banner:
                    match.RawBid.AdMarkup = match.ToImageLinkMarkup(bidContext, new UriBuilder
                    {
                        Scheme = httpContext.Request.Scheme,
                        Host = httpContext.Request.Host.Value,
                    }.Uri);
                    break;
                case ContentType.Video:
                    match.RawBid.AdMarkup = match.ToVast();
                    break;
                default:
                    return null;
            }

            return match.RawBid;
        }
    }
}