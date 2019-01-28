using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Class for managing campaigns
    /// </summary>
    public class CampaignManager
    {
        IMessageSubscriber<EntityEventMessage> _eventSubscriber;
        IStorageRepository<Campaign> _campaignRepo;
        IStorageRepository<Creative> _creativeRepo;
        ILogger _log;

        ConcurrentDictionary<StorageKey, Campaign> _campaigns = new ConcurrentDictionary<StorageKey, Campaign>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="storageManager"></param>
        /// <param name="log"></param>
        public CampaignManager(IMessageFactory messageFactory, IStorageManager storageManager, ILogger log)
        {
            _log = log;

            // Setup repos
            _campaignRepo = storageManager.GetRepository<Campaign>();
            _creativeRepo = storageManager.GetRepository<Creative>();

            // Setup message loop
            _eventSubscriber = messageFactory.CreateSubscriber<EntityEventMessage>("bidding", 0, messageFactory.WildcardFilter);
            _eventSubscriber.OnReceive = TrackEntities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaign"></param>
        public async Task AddCampaign(Campaign campaign)
        {
            campaign = _campaigns.GetOrAdd(campaign.Key, campaign);
            campaign.Creatives.Clear();

            foreach (var creativeId in campaign.CreativeIds)
            {
                var cr = await _creativeRepo.Get(new StringStorageKey(creativeId));
                foreach (var content in cr.Contents)
                    if (content.Filter == null)
                        content.HydrateFilter();

                campaign.Creatives.Add(cr);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Campaign> Campaigns { get; set; } = new List<Campaign>();

        async Task TrackEntities(EntityEventMessage entityEvent)
        {
            switch (entityEvent.Body.EventType)
            {
                case EventType.EntityAdd:
                case EventType.EntityUpdate:
                    switch (entityEvent.Body.EntityType)
                    {
                        case EntityType.Campaign:
                            var campaign = await _campaignRepo.Get(new StringStorageKey(entityEvent.Body.EntityId));
                            await AddCampaign(campaign);

                            Campaigns = _campaigns.Values.ToList();

                            break;
                        case EntityType.Creative:
                            var creative = await _creativeRepo.Get(new StringStorageKey(entityEvent.Body.EntityId));
                            foreach (var content in creative.Contents)
                                if (content.Filter == null)
                                    content.HydrateFilter();

                            foreach (var camp in _campaigns.Values.ToArray())
                                if (camp.Creatives.Any(cr => cr.Key == creative.Key))
                                {
                                    camp.Creatives = camp.Creatives.Where(cr => cr.Key != creative.Key).ToList();
                                    camp.Creatives.Add(creative);
                                }

                            Campaigns = _campaigns.Values.ToList();
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
    }
}