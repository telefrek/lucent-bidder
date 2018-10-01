using Lucent.Common.Entities;
using Lucent.Common.Messaging;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Orchestration
{
    public interface ICampaignProcessor
    {
        void Start();
        void Stop();
    }

    public class CampaignManager : ICampaignProcessor
    {
        IMessageFactory _factory;
        IMessageSubscriber<LucentMessage<Campaign>> _campaignUpdateSubscriber;
        ILogger<CampaignManager> _log;
        IStorageManager _storage;
        ILucentRepository<Campaign> _campaignRepo;

        public CampaignManager(IMessageFactory factory,
            IStorageManager storage, ILogger<CampaignManager> log)
        {
            _log = log;
            _storage = storage;
            _campaignRepo = storage.GetRepository<Campaign>();
            _factory = factory;
        }

        public void Start()
        {
            _log.LogInformation("Staring campaign processor");
            _campaignUpdateSubscriber = _factory.CreateSubscriber<LucentMessage<Campaign>>("campaign-updates", 0);
            _campaignUpdateSubscriber.OnReceive = ProcessMessage;
        }

        void ProcessMessage(LucentMessage<Campaign> message)
        {
            _log.LogInformation("Received campaign update event");

            if(message.Body != null)
            {
                var success = _campaignRepo.TryUpdate(message.Body).Result ||
                    _campaignRepo.TryInsert(message.Body).Result;

                _log.LogInformation("Storage of campaign {0} : {1}",
                    message.Body.Id, success);
            }
            else
            {
                _log.LogInformation("No campaign data in message");
            }
        }

        public void Stop()
        {
            _campaignUpdateSubscriber.Dispose();
        }
    }
}