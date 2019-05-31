using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Class to manage bidding infrastructure, events and common objects
    /// </summary>
    public class BiddingManager : IBiddingManager
    {
        ILogger<BiddingManager> _log;
        ISerializationContext _serializationContext;
        IMessageFactory _messageFactory;
        IBidFactory _bidFactory;
        IMessageSubscriber<EntityEventMessage> _entityEvents;
        IBudgetManager _budgetManager;
        string _exchangeId;
        Exchange _exchange;
        bool _isBudgetExhausted = false;

        IBudgetCache _budgetCache;
        StorageCache _storageCache;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializationContext"></param>
        /// <param name="storageCache"></param>
        /// <param name="messageFactory"></param>
        /// <param name="bidFactory"></param>
        /// <param name="budgetManager"></param>
        /// <param name="budgetCache"></param>
        public BiddingManager(ILogger<BiddingManager> logger, ISerializationContext serializationContext, StorageCache storageCache, IMessageFactory messageFactory, IBidFactory bidFactory, IBudgetManager budgetManager, IBudgetCache budgetCache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _storageCache = storageCache;
            _messageFactory = messageFactory;
            _bidFactory = bidFactory;
            _entityEvents = _messageFactory.CreateSubscriber<EntityEventMessage>(Topics.BIDDING, _messageFactory.WildcardFilter);
            _entityEvents.OnReceive = HandleMessage;
            _budgetManager = budgetManager;
            _budgetCache = budgetCache;

            _budgetManager.RegisterHandler(new BudgetEventHandler
            {
                IsMatch = (e) => { return e.EntityId == _exchangeId; },
                HandleAsync = async (e) =>
                {
                    var status = await _budgetCache.TryGetRemaining(_exchangeId);
                    if (status.Successful)
                    {
                        LocalBudget.Get(_exchangeId).Budget.Sync(status.Remaining);
                        _isBudgetExhausted = status.Remaining <= 0;
                    }
                    else
                    {
                        _isBudgetExhausted = true;
                        _log.LogWarning("Failed to sync exchange budget for {0}, stopping to be safe", _exchangeId);
                    }

                    _log.LogInformation("Budget change for exchange : {0} ({1})", _exchangeId, _isBudgetExhausted);
                }
            });
        }

        async Task<Campaign> FillCampaign(Campaign campaign)
        {
            foreach (var creativeId in campaign.CreativeIds)
            {
                var cr = await _storageCache.Get<Creative>(new StringStorageKey(creativeId), true);
                foreach (var content in cr.Contents ?? new CreativeContent[0])
                    if (content.Filter == null)
                        content.HydrateFilter();

                campaign.Creatives.Add(cr);
            }

            return campaign;
        }

        /// <summary>
        /// Entity change handler
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task HandleMessage(EntityEventMessage message)
        {
            if (message.Body != null && _exchange != null)
            {
                try
                {
                    var id = message.Body.EntityId;
                    switch (message.Body.EntityType)
                    {
                        case EntityType.Campaign:
                            switch (message.Body.EventType)
                            {
                                case EventType.EntityAdd:
                                case EventType.EntityUpdate:
                                    if (_exchange.CampaignIds != null && _exchange.CampaignIds.Any(c => c.Equals(id)))
                                    {
                                        _log.LogInformation("Updating bidder for campaign : {0}", id);
                                        var entity = await _storageCache.Get<Campaign>(new StringStorageKey(id), true);
                                        Bidders.RemoveAll(b => b.Campaign.Id.Equals(id));
                                        if (entity != null)
                                        {
                                            _log.LogInformation("Added bidder for campaign : {0}", id);
                                            Bidders.Add(_bidFactory.CreateBidder(await FillCampaign(entity)));
                                        }
                                        break;
                                    }

                                    return;
                                case EventType.EntityDelete:
                                    _log.LogInformation("Removing bidder for campaign : {0}", id);
                                    Bidders.RemoveAll(b => b.Campaign.Id.Equals(id));
                                    break;
                            }
                            break;
                        case EntityType.Exchange:
                            if (id == _exchangeId)
                            {
                                switch (message.Body.EventType)
                                {
                                    case EventType.EntityUpdate:
                                        _exchange = await _storageCache.Get<Exchange>(new GuidStorageKey(Guid.Parse(_exchangeId)), true) ?? _exchange;
                                        await UpdateCampaigns();
                                        break;
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    _log.LogWarning(e, "Failed to process message {0}", message.MessageId ?? "unknown");
                }
            }
        }

        /// <inheritdoc/>
        public async Task Initialize(AdExchange adExchange)
        {
            _exchangeId = adExchange.ExchangeId.ToString();
            _exchange = await _storageCache.Get<Exchange>(new GuidStorageKey(Guid.Parse(_exchangeId)), true);
            if (_exchange != null && _exchange.CampaignIds != null)
                await UpdateCampaigns();
            else
                Bidders.Clear();
        }

        async Task UpdateCampaigns()
        {
            // Remove missing campaigns
            foreach (var removedId in Bidders.Select(b => b.Campaign.Id).Where(id => !(_exchange.CampaignIds ?? new string[0]).Contains(id)))
                Bidders.RemoveAll(b => b.Campaign.Id.Equals(removedId));

            // Update the rest
            foreach (var campaignId in _exchange.CampaignIds ?? new string[0])
            {
                var campaign = await _storageCache.Get<Campaign>(new StringStorageKey(campaignId), true);
                if (campaign != null)
                {
                    Bidders.RemoveAll(b => b.Campaign.Id.Equals(campaignId));
                    Bidders.Add(_bidFactory.CreateBidder(await FillCampaign(campaign)));
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanBid()
        {
            if (_isBudgetExhausted = LocalBudget.Get(_exchangeId).Budget.GetDouble() <= 0)
            {
                BidCounters.NoBidReason.WithLabels("no_exchange_budget").Inc();
                await _budgetManager.RequestAdditional(_exchangeId, EntityType.Exchange);
                return false;
            }

            return !_isBudgetExhausted;
        }

        /// <summary>
        /// Get the active bidders
        /// </summary>
        /// <returns></returns>
        public List<ICampaignBidder> Bidders { get; private set; } = new List<ICampaignBidder>();
    }
}