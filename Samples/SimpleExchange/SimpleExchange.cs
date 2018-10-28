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
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucent.Samples.SimpleExchange
{
    /// <summary>
    /// Implementation of a very basic exchange
    /// </summary>
    public class SimpleExchange : IAdExchange
    {
        IStorageManager _storageManager;
        IMessageFactory _messageFactory;
        ISerializationRegistry _serializationRegistry;
        IBidFactory _bidFactory;
        IMessageSubscriber<LucentMessage<Campaign>> _campaignSub;
        ILogger<SimpleExchange> _log;

        List<ICampaignBidder> _bidders = new List<ICampaignBidder>();

        public void Initialize(IServiceProvider provider)
        {
            _storageManager = provider.GetService<IStorageManager>();
            _messageFactory = provider.GetService<IMessageFactory>();
            _serializationRegistry = provider.GetService<ISerializationRegistry>();
            _bidFactory = provider.GetService<IBidFactory>();
            _log = provider.GetService<ILogger<SimpleExchange>>();

            // Get the current campaign bidders
            _bidders.AddRange(_storageManager.GetRepository<Campaign>().Get().Result.Select(c => _bidFactory.CreateBidder(c)));

            // Setup the bid state update
            _campaignSub = _messageFactory.CreateSubscriber<LucentMessage<Campaign>>("bidstate", 0, "campaign.#");
            _campaignSub.OnReceive = ProcessMessage;
        }

        void ProcessMessage(LucentMessage<Campaign> message)
        {
            _log.LogInformation("Received new message {0}", message.MessageId);
            var camp = message.Body;
            if (camp != null)
            {
                Task.Factory.StartNew(async () =>
                {
                    _log.LogInformation("Campaign {0} updateded, reloading", camp.Id);
                    var bidder = _bidders.FirstOrDefault(b => b.Campaign.Id.Equals(camp.Id, StringComparison.InvariantCultureIgnoreCase));
                    if (bidder != null)
                        _bidders.Remove(bidder);

                    var newCamp = await _storageManager.GetRepository<Campaign>().Get(camp.Id);
                    _bidders.Add(_bidFactory.CreateBidder(newCamp));
                }).Unwrap();
            }
        }

        /// <inheritdoc/>
        public bool SuppressBOM => true;

        /// <inheritdoc/>
        public string Name => "SimpleExchange";

        /// <inheritdoc/>
        public int Order => int.MinValue;

        /// <inheritdoc/>
        public Guid ExchangeId => Guid.Parse("9363aae4-a305-43e6-b0be-a2f5cda1edff");

        /// <inheritdoc/>
        public async Task<BidResponse> Bid(BidRequest request, HttpContext httpContext)
        {
            if (_bidders.Count == 0)
                return null;

            var resp = new BidResponse
            {
                NoBidReason = NoBidReason.SuspectedNonHuman,
                Id = request.Id,
                CorrelationId = SequentialGuid.NextGuid().ToString(),
            };

            var bids = _bidders.Select(b => b.BidAsync(request, httpContext));

            // Probably make this not wait on everything...
            await Task.WhenAll(bids);
            var seats = new List<SeatBid>();

            foreach (var bid in bids.Where(b => b.Result.Length > 0).Select(b => b.Result))
            {
                var c = bid.First().Campaign;

                var seat = new SeatBid
                {
                    BuyerId = c.BuyerId,
                    Bids = bid.Select(b => FormatBid(b)).ToArray()
                };

                if (seat.Bids.Length > 0)
                    seats.Add(seat);
            }

            if (seats.Count > 0)
            {
                resp.Bids = seats.ToArray();
                return resp;
            }

            return null;
        }

        /// <inheritdoc/>
        public bool IsMatch(HttpContext context) => true;

        /// <inheritdoc/>
        public Bid FormatBid(BidMatch match)
        {
            // Format and stash/attach markup
            switch (match.Content.ContentType)
            {
                case ContentType.Banner:
                    match.RawBid.AdMarkup = match.ToImageLinkMarkup();
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