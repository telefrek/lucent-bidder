using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Storage;

namespace Lucent.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdatingCollection<T> where T : IStorageEntity, new()
    {
        IStorageRepository<T> _storageRepository;
        IMessageSubscriber<EntityEventMessage> _entitySubscriber;
        EntityType _entityType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="storageManager"></param>
        /// <param name="entityType"></param>
        public UpdatingCollection(IMessageFactory messageFactory, IStorageManager storageManager, EntityType entityType)
        {
            _entitySubscriber = messageFactory.CreateSubscriber<EntityEventMessage>(Topics.BIDDING, 0, messageFactory.WildcardFilter);
            _entitySubscriber.OnReceive = HandleMessageAsync;
            _storageRepository = storageManager.GetRepository<T>();
            _entityType = entityType;

            Entities = _storageRepository.GetAll().Result.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<T> Entities { get; private set; } = new List<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Func<Task> OnUpdate { get; set; } = () => Task.CompletedTask;

        /// <summary>
        /// Update the entity collection
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task HandleMessageAsync(EntityEventMessage message)
        {
            if (message.Body.EntityType == _entityType)
            {
                Entities = (await _storageRepository.GetAll()).ToList();
                await OnUpdate();
            }
        }
    }
}