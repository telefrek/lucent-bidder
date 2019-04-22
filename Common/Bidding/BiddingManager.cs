using System;
using System.Collections.Generic;
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
        IStorageManager _storageManager;
        IMessageFactory _messageFactory;
        IBidFactory _bidFactory;
        IMessageSubscriber<EntityEventMessage> _entityEvents;
        IBudgetManager _budgetManager;
        string _exchangeId;
        bool _isBudgetExhausted = false;

        IStorageRepository<Campaign> _campaignRepo;
        IStorageRepository<Creative> _creativeRepo;
        IBudgetCache _budgetCache;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializationContext"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="bidFactory"></param>
        /// <param name="budgetManager"></param>
        /// <param name="budgetCache"></param>
        public BiddingManager(ILogger<BiddingManager> logger, ISerializationContext serializationContext, IStorageManager storageManager, IMessageFactory messageFactory, IBidFactory bidFactory, IBudgetManager budgetManager, IBudgetCache budgetCache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _storageManager = storageManager;
            _messageFactory = messageFactory;
            _bidFactory = bidFactory;
            _entityEvents = _messageFactory.CreateSubscriber<EntityEventMessage>(Topics.BIDDING, _messageFactory.WildcardFilter);
            _entityEvents.OnReceive = HandleMessage;
            _creativeRepo = storageManager.GetRepository<Creative>();
            _budgetManager = budgetManager;
            _budgetCache = budgetCache;

            _budgetManager.RegisterHandler(new BudgetEventHandler
            {
                IsMatch = (e)=>{return e.EntityId == _exchangeId;},
                HandleAsync = async (e)=>
                {
                    var rem = await _budgetCache.TryGetBudget(_exchangeId);
                    LocalBudget.Get(_exchangeId).Last = rem;

                    _isBudgetExhausted = rem <= 0d;
                    _log.LogInformation("Budget change for exchange : {0} ({1})", _exchangeId, _isBudgetExhausted);
                }
            });

            foreach (var campaign in _storageManager.GetRepository<Campaign>().GetAll().Result)
            {
                _log.LogInformation("Added bidder for campaign : {0}", campaign.Id);
                Bidders.Add(_bidFactory.CreateBidder(FillCampaign(campaign).Result));
            }
        }

        async Task<Campaign> FillCampaign(Campaign campaign)
        {
            foreach (var creativeId in campaign.CreativeIds)
            {
                var cr = await _creativeRepo.Get(new StringStorageKey(creativeId));
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
            if (message.Body != null)
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
                                    _log.LogInformation("Updating bidder for campaign : {0}", id);
                                    var entity = await _storageManager.GetRepository<Campaign>().Get(new StringStorageKey(id));
                                    Bidders.RemoveAll(b => b.Campaign.Id.Equals(id));
                                    if (entity != null)
                                    {
                                        _log.LogInformation("Added bidder for campaign : {0}", id);
                                        Bidders.Add(_bidFactory.CreateBidder(entity));
                                    }
                                    break;
                                case EventType.EntityDelete:
                                    _log.LogInformation("Removing bidder for campaign : {0}", id);
                                    Bidders.RemoveAll(b => b.Campaign.Id.Equals(id));
                                    break;
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
        public Task Initialize(AdExchange exchange)
        {
            _exchangeId = exchange.ExchangeId.ToString();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> CanBid()
        {
            if (_isBudgetExhausted |= LocalBudget.Get(_exchangeId).IsExhausted())
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