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
        IStorageRepository<Exchange, Guid> _storageRepository;
        IMessageSubscriber<EntityEventMessage> _subscriber;

        object _syncLock = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageFactory"></param>
        /// <param name="storageManager"></param>
        public ExchangeRegistry(ILogger<ExchangeRegistry> logger, IMessageFactory messageFactory, IStorageManager storageManager)
        {
            _log = logger;
            _messageFactory = messageFactory;
            _storageRepository = storageManager.GetRepository<Exchange, Guid>();
            _subscriber = messageFactory.CreateSubscriber<EntityEventMessage>("exchanges", 0, "#");

            _subscriber.OnReceive = async (message) =>
            {
                var evt = message.Body;

                if (evt.EntityType != EntityType.Exchange) return;

                switch (evt.EventType)
                {
                    case EventType.EntityAdd:
                    case EventType.EntityUpdate:
                        Guid id;
                        if (!Guid.TryParse(evt.EntityId, out id)) return;
                        var entity = await _storageRepository.Get(id);
                        if (entity.Instance != null)
                        {
                            _log.LogInformation("Loaded exchange : {0}", entity.Id);
                            _exchangeMap[evt.EntityId] = entity.Instance;
                        }
                        break;
                    case EventType.EntityDelete:
                        if (_exchangeMap.Remove(evt.EntityId))
                            _log.LogInformation("Unloaded : {0}", evt.EntityId);
                        break;
                }
            };
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
            foreach (var exchange in await _storageRepository.GetAll())
            {
                if (exchange.Instance != null)
                {
                    _log.LogInformation("Loaded exchange : {0}", exchange.Id);
                    _exchangeMap[exchange.Id.ToString()] = exchange.Instance;
                }
            }
        }
    }
}