using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;
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
        public async Task<BidResponse> Bid(BidRequest request)
        {
            if (_bidders.Count == 0)
                return null;

            var resp = new BidResponse
            {
                NoBidReason = NoBidReason.SuspectedNonHuman,
                Id = request.Id,
                CorrelationId = SequentialGuid.NextGuid().ToString(),
            };

            var bids = _bidders.Select(b => new { Bidder = b, Imp = b.FilterImpressions(request) })
                .Where(b => (b.Imp ?? new Impression[0]).Length > 0)
                .Select(b => new { Bids = b.Imp.Select(i => b.Bidder.BidAsync(i)).ToArray(), Bidder = b.Bidder });

            foreach (var b in bids)
                await Task.WhenAll(b.Bids);

            resp.Bids = bids.Select(b => new SeatBid { BuyerId = b.Bidder.Campaign.Id, Bids = b.Bids.Select(t => t.Result).ToArray() }).ToArray();

            return resp;
        }

        /// <inheritdoc/>
        public bool IsMatch(HttpContext context) => true;
    }
}