using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Exchange repository implementation
    /// </summary>
    public class ExchangeRegistry : IExchangeRegistry
    {
        ILogger<ExchangeRegistry> _log;
        Dictionary<string, AdExchange> _exchangeMap = new Dictionary<string, AdExchange>();
        IMessageFactory _messageFactory;
        IStorageRepository<Exchange> _storageRepository;
        IMessageSubscriber<EntityEventMessage> _subscriber;
        IServiceProvider _serviceProvider;

        object _syncLock = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageFactory"></param>
        /// <param name="storageManager"></param>
        /// <param name="provider"></param>
        public ExchangeRegistry(ILogger<ExchangeRegistry> logger, IMessageFactory messageFactory, IStorageManager storageManager, IServiceProvider provider)
        {
            _log = logger;
            _messageFactory = messageFactory;
            _storageRepository = storageManager.GetRepository<Exchange>();
            _subscriber = messageFactory.CreateSubscriber<EntityEventMessage>(Topics.BIDDING, messageFactory.WildcardFilter);
            _subscriber.OnReceive += WatchExchanges;
            _serviceProvider = provider;
            Task.Run(Initialize);
        }

        async Task WatchExchanges(EntityEventMessage message)
        {
            _log.LogInformation("New message {0} ({1} {2})", message.MessageId ?? "none", message.Body.EventType, message.Body.EntityType);
            var evt = message.Body;

            if (evt.EntityType != EntityType.Exchange) return;

            switch (evt.EventType)
            {
                case EventType.EntityAdd:
                case EventType.EntityUpdate:
                    Guid id;
                    if (!Guid.TryParse(evt.EntityId, out id)) return;
                    var entity = await _storageRepository.Get(new GuidStorageKey(id));
                    if(entity == null) return;

                    if (entity.Code != null)
                        await entity.LoadExchange(_serviceProvider);

                    if (entity.Instance != null)
                    {
                        _log.LogInformation("Loaded exchange : {0}", entity.Id);
                        _exchangeMap[entity.Id.ToString()] = entity.Instance;
                    }
                    break;
                case EventType.EntityDelete:
                    if (_exchangeMap.Remove(evt.EntityId))
                        _log.LogInformation("Unloaded : {0}", evt.EntityId);
                    break;
            }
        }

        /// <inheritdoc/>
        public AdExchange GetExchange(HttpContext context)
        {
            var exchgId = context.Request.Query.FirstOrDefault(s => s.Key.Equals("exchg", StringComparison.InvariantCultureIgnoreCase));
            if (exchgId.Value.Any())
                return _exchangeMap.GetValueOrDefault(exchgId.Value.First(), null);
            return null;
        }

        /// <inheritdoc/>
        public bool HasExchange(Guid id) => _exchangeMap.ContainsKey(id.ToString());

        /// <inheritdoc/>
        public async Task Initialize()
        {
            var exchanges = await _storageRepository.GetAll();
            foreach (var entity in exchanges)
            {
                if (entity.Code != null)
                    await entity.LoadExchange(_serviceProvider);
                if (entity.Instance != null)
                {
                    _log.LogInformation("Loaded exchange : {0}", entity.Id);
                    _exchangeMap[entity.Id.ToString()] = entity.Instance;
                }
            }
        }
    }
}