using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Entities.Events;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Entities
{
    /// <inheritdoc/>
    public class EntityWatcher : IEntityWatcher
    {
        IMessageFactory _messageFactory;
        IMessageSubscriber<EntityEventMessage> _messageSubscriber;
        ILogger _log;

        ConcurrentDictionary<string, List<Func<EntityEvent, Task>>> _hooks = new ConcurrentDictionary<string, List<Func<EntityEvent, Task>>>();

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="logger"></param>
        public EntityWatcher(IMessageFactory messageFactory, ILogger<EntityWatcher> logger)
        {
            _log = logger;
            _messageFactory = messageFactory;

            _messageSubscriber = messageFactory.CreateSubscriber<EntityEventMessage>(Topics.ENTITIES, messageFactory.WildcardFilter);
            _messageSubscriber.OnReceive += OnMessageReceived;
        }

        async Task OnMessageReceived(EntityEventMessage message)
        {
            var evt = message.Body;

            if (evt != null && _hooks.ContainsKey(evt.EntityId))
            {
                foreach (var hook in _hooks[evt.EntityId])
                    try
                    {
                        await hook(evt);
                    }
                    catch (Exception e)
                    {
                        _log.LogError(e, "Failed to process hook");
                    }
            }
        }

        /// <inheritdoc/>
        public void Register(string entityId, Func<EntityEvent, Task> onUpdate)
        {
            _hooks.AddOrUpdate(entityId, new List<Func<EntityEvent, Task>>() { onUpdate }, (s, pl) =>
               {
                   pl.Add(onUpdate);
                   return pl;
               });
        }
    }
}