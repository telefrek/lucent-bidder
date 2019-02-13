using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

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
        List<T> _entities;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageFactory"></param>
        /// <param name="storageManager"></param>
        /// <param name="logger"></param>
        /// <param name="entityType"></param>
        public UpdatingCollection(IMessageFactory messageFactory, IStorageManager storageManager, ILogger logger, EntityType entityType)
        {
            try
            {
                logger.LogInformation("getting subscriber");
                _entitySubscriber = messageFactory.CreateSubscriber<EntityEventMessage>(Topics.BIDDING, 0, messageFactory.WildcardFilter);
                _entitySubscriber.OnReceive = HandleMessageAsync;

                logger.LogInformation("getting repo");
                _storageRepository = storageManager.GetRepository<T>();


                logger.LogInformation("setting etype");
                _entityType = entityType;

                logger.LogInformation("getting all entities {0}", _storageRepository == null); ;
                _entities = _storageRepository.GetAll().Result.ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create collection for {0}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<T> Entities { get => _entities; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Func<Task> OnUpdate { get; set; }

        /// <summary>
        /// Update the entity collection
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task HandleMessageAsync(EntityEventMessage message)
        {
            if (message.Body.EntityType == _entityType)
            {
                _entities = (await _storageRepository.GetAll()).ToList();
                if (OnUpdate != null)
                    await OnUpdate();
            }
        }
    }
}