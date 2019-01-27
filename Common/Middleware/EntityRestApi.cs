using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Test the rest api!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class EntityRestApi<T, K> where T : class, IStorageEntity<K>, new()
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly IStorageManager _storageManager;

        /// <summary>
        /// 
        /// </summary>
        protected readonly ISerializationContext _serializationContext;

        /// <summary>
        /// 
        /// </summary>
        protected readonly ILogger<EntityRestApi<T, K>> _logger;

        /// <summary>
        /// 
        /// </summary>
        protected readonly IStorageRepository<T, K> _entityRepository;

        /// <summary>
        /// 
        /// </summary>
        protected readonly IMessageFactory _messageFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        public EntityRestApi(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<EntityRestApi<T, K>> logger)
        {
            _storageManager = storageManager;
            _entityRepository = storageManager.GetRepository<T, K>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;

            _messageFactory.CreateSubscriber<LucentMessage<T>>("entities", 0, _messageFactory.WildcardFilter).OnReceive += UpdateEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityEvent"></param>
        /// <returns></returns>
        async Task UpdateEntity(LucentMessage<T> entityEvent)
        {
            if (entityEvent.Body != null)
            {
                var evt = new EntityEvent
                {
                    EntityType = (EntityType)Enum.Parse(typeof(EntityType), typeof(T).Name),
                    EntityId = entityEvent.Body.Id.ToString(),
                };

                // This is awful, don't do this for real
                if (await _entityRepository.TryUpdate(entityEvent.Body))
                    evt.EventType = EventType.EntityUpdate;
                else if (await _entityRepository.TryInsert(entityEvent.Body))
                    evt.EventType = EventType.EntityAdd;
                else if (await _entityRepository.TryRemove(entityEvent.Body))
                    evt.EventType = EventType.EntityDelete;

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = typeof(T).Name.ToLowerInvariant();

                    await _messageFactory.CreatePublisher("bidding").TryPublish(msg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual async Task<T> ReadEntity(HttpContext httpContext)
        {
            T entity = await _serializationContext.ReadAs<T>(httpContext);

            if (entity == null && httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length > 0)
            {
                var id = httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                var key = (K)Convert.ChangeType(id, typeof(K));
                if (!key.IsNullOrDefault())
                    return await _entityRepository.Get(key);
            }

            return entity;
        }


        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var entity = await ReadEntity(httpContext);
            if (entity != null)
            {
                // Validate
                var evt = new EntityEvent
                {
                    EntityType = (EntityType)Enum.Parse(typeof(EntityType), typeof(T).Name),
                    EntityId = entity.Id.ToString(),
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "post":
                        if (await _entityRepository.TryInsert(entity))
                        {
                            httpContext.Response.StatusCode = 201;
                            evt.EventType = EventType.EntityAdd;
                            evt.EntityId = entity.Id.ToString();
                            await _serializationContext.WriteTo(httpContext, entity);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "put":
                    case "patch":
                        if (await _entityRepository.TryUpdate(entity))
                        {
                            httpContext.Response.StatusCode = 202;
                            evt.EventType = EventType.EntityUpdate;
                            await _serializationContext.WriteTo(httpContext, entity);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "delete":
                        if (await _entityRepository.TryRemove(entity))
                        {
                            evt.EventType = EventType.EntityDelete;
                            httpContext.Response.StatusCode = 204;
                        }
                        else
                            httpContext.Response.StatusCode = 404;
                        break;
                }

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = typeof(T).Name.ToLowerInvariant();
                    await _messageFactory.CreatePublisher("bidding").TryPublish(msg);

                    var sync = _messageFactory.CreateMessage<LucentMessage<T>>();
                    sync.Body = entity;
                    sync.Route = typeof(T).Name.ToLowerInvariant();
                    await _messageFactory.CreatePublisher("entities").TryBroadcast(msg);
                }
            }
        }
    }
}