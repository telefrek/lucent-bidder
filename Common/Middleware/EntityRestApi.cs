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
    public class EntityRestApi<T> where T : class, IStorageEntity, new()
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
        protected readonly ILogger<EntityRestApi<T>> _log;

        /// <summary>
        /// 
        /// </summary>
        protected readonly IStorageRepository<T> _entityRepository;

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
        public EntityRestApi(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<EntityRestApi<T>> logger)
        {
            _storageManager = storageManager;
            _entityRepository = storageManager.GetRepository<T>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _log = logger;

            _messageFactory.CreateSubscriber<LucentMessage<T>>(Topics.ENTITIES, 0, _messageFactory.WildcardFilter).OnReceive += UpdateEntity;
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
                    EntityId = entityEvent.Body.Key.ToString(),
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

                    await _messageFactory.CreatePublisher(Topics.BIDDING).TryPublish(msg);
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
            var segments = httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (entity == null && segments.Length > 0)
            {
                var id = segments[segments.Length - 1];
                var t = new T();
                t.Key.Parse(id);
                return await _entityRepository.Get(t.Key);
            }

            return entity;
        }


        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var entity = await ReadEntity(httpContext);
                if (entity != null)
                {
                    // Validate
                    var evt = new EntityEvent
                    {
                        EntityType = (EntityType)Enum.Parse(typeof(EntityType), typeof(T).Name),
                        EntityId = entity.Key.ToString(),
                    };

                    switch (httpContext.Request.Method.ToLowerInvariant())
                    {
                        case "get":
                            httpContext.Response.StatusCode = entity != null ? 200 : 404;
                            if (entity != null)
                                await _serializationContext.WriteTo(httpContext, entity);
                            break;
                        case "post":
                            if (await _entityRepository.TryInsert(entity))
                            {
                                httpContext.Response.StatusCode = 201;
                                evt.EventType = EventType.EntityAdd;
                                evt.EntityId = entity.Key.ToString();
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
                        await _messageFactory.CreatePublisher(Topics.BIDDING).TryPublish(msg);

                        var sync = _messageFactory.CreateMessage<LucentMessage<T>>();
                        sync.Body = entity;
                        sync.Route = typeof(T).Name.ToLowerInvariant();
                        await _messageFactory.CreatePublisher(Topics.ENTITIES).TryBroadcast(msg);
                    }
                }
                else if (httpContext.Request.Method.ToLowerInvariant() == "get")
                {
                    var entities = await _entityRepository.GetAll();
                    httpContext.Response.StatusCode = entities.Count > 0 ? 200 : 204;
                    if (entities.Count > 0)
                        await _serializationContext.WriteTo(httpContext, entities);
                }
                else
                    httpContext.Response.StatusCode = 404;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed rest API call");
                httpContext.Response.StatusCode = 500;
            }

        }
    }
}