using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Authentication;
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
        protected readonly ISerializationContext _serializationContext;

        /// <summary>
        /// 
        /// </summary>
        protected readonly ILogger<EntityRestApi<T>> _log;

        /// <summary>
        /// 
        /// </summary>
        protected readonly IMessageFactory _messageFactory;

        /// <summary>
        /// 
        /// </summary>
        protected readonly IMessagePublisher _bidderPublisher;

        /// <summary>
        /// 
        /// </summary>
        protected StorageCache _storageCache;

        IBudgetCache _budgetCache;
        IStorageRepository<T> _entityRepository;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageCache"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <param name="budgetCache"></param>
        public EntityRestApi(RequestDelegate next, StorageCache storageCache, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<EntityRestApi<T>> logger, IBudgetCache budgetCache)
        {
            _storageCache = storageCache;
            _entityRepository = storageManager.GetRepository<T>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _log = logger;
            _budgetCache = budgetCache;

            _messageFactory.CreateSubscriber<LucentMessage<T>>(Topics.ENTITIES, _messageFactory.WildcardFilter).OnReceive += UpdateEntity;
            _bidderPublisher = _messageFactory.CreatePublisher(Topics.BIDDING);
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

                if (entityEvent.Body.EntityType == evt.EntityType)
                {
                    if (await _storageCache.TryUpdate(entityEvent.Body))
                        evt.EventType = EventType.EntityUpdate;
                    else if (await _storageCache.TryInsert(entityEvent.Body))
                        evt.EventType = EventType.EntityAdd;
                    else if (await _storageCache.TryRemove(entityEvent.Body, true))
                        evt.EventType = EventType.EntityDelete;
                }

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = typeof(T).Name.ToLowerInvariant();

                    await _bidderPublisher.TryPublish(msg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual async Task<T> ReadEntity(HttpContext httpContext) => await _serializationContext.ReadAs<T>(httpContext);

        /// <summary>
        /// Hook for post delete actions
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="entity"></param>
        protected virtual void PostDelete(HttpContext httpContext, T entity) { }


        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var res = await httpContext.AuthenticateAsync("Bearer");
                if (!res.Succeeded)
                {
                    httpContext.Response.StatusCode = 403;
                    return;
                }

                var segments = httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var entity = default(T);

                // Validate
                var evt = new EntityEvent
                {
                    EntityType = (EntityType)Enum.Parse(typeof(EntityType), typeof(T).Name),
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "get":
                        if (segments.Length > 0)
                        {
                            var t = new T();
                            t.Key.Parse(segments.Last());
                            entity = await _storageCache.Get<T>(t.Key, true);
                            if (entity != null)
                            {
                                httpContext.Response.StatusCode = 200;
                                httpContext.Response.Headers.Add("X-LUCENT-ETAG", entity.ETag);
                                await _serializationContext.WriteTo(httpContext, entity);
                            }
                            else
                                httpContext.Response.StatusCode = 404;
                        }
                        else
                        {
                            _log.LogInformation("Getting all for {0}", typeof(T).Name);
                            var entities = await _entityRepository.GetAll();
                            _log.LogInformation("Read {0} entities", entities.Count);
                            httpContext.Response.StatusCode = entities.Count > 0 ? 200 : 204;
                            if (entities.Count > 0)
                                await _serializationContext.WriteTo(httpContext, entities);
                        }
                        break;
                    case "post":
                        _log.LogInformation("POST {0}: Reading entity", typeof(T).Name);
                        entity = await ReadEntity(httpContext);
                        if (entity != null)
                        {
                            _log.LogInformation("POST {0}: Inserting entity", typeof(T).Name);
                            if (await _storageCache.TryInsert(entity))
                            {
                                httpContext.Response.StatusCode = 201;
                                evt.EventType = EventType.EntityAdd;
                                evt.EntityId = entity.Key.ToString();
                                httpContext.Response.Headers.Add("X-LUCENT-ETAG", entity.ETag);
                                _log.LogInformation("Sending response");
                                await _serializationContext.WriteTo(httpContext, entity);
                            }
                            else
                                httpContext.Response.StatusCode = 409;
                        }
                        else
                        {
                            httpContext.Response.StatusCode = 400;
                        }
                        break;
                    case "put":
                    case "patch":
                        entity = await ReadEntity(httpContext);
                        if (entity != null)
                        {
                            entity.ETag = httpContext.Request.Headers["X-LUCENT-ETAG"];
                            if (await _storageCache.TryUpdate(entity))
                            {
                                httpContext.Response.StatusCode = 202;
                                evt.EntityId = entity.Key.ToString();
                                evt.EventType = EventType.EntityUpdate;
                                httpContext.Response.Headers.Add("X-LUCENT-ETAG", entity.ETag);
                                await _serializationContext.WriteTo(httpContext, entity);
                            }
                            else
                                httpContext.Response.StatusCode = 409;
                        }
                        else
                        {
                            httpContext.Response.StatusCode = 400;
                        }
                        break;
                    case "delete":
                        if (segments.Length > 0)
                        {
                            var t = new T();
                            t.Key.Parse(segments.Last());
                            entity = await _storageCache.Get<T>(t.Key, true);

                            if (entity != null)
                            {
                                entity.ETag = httpContext.Request.Headers["X-LUCENT-ETAG"];
                                if (await _storageCache.TryRemove(entity, true))
                                {
                                    PostDelete(httpContext, entity);
                                    evt.EntityId = entity.Key.ToString();
                                    evt.EventType = EventType.EntityDelete;
                                    httpContext.Response.StatusCode = 204;
                                }
                                else
                                    httpContext.Response.StatusCode = 404;
                            }
                            else
                                httpContext.Response.StatusCode = 400;
                        }
                        else
                            httpContext.Response.StatusCode = 400;
                        break;
                    default:
                        httpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                        break;
                }

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    _log.LogInformation("Publishing {0} to {1}", evt.EventType, evt.EntityId);
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
            catch (Exception e)
            {
                _log.LogError(e, "Failed rest API call");
                httpContext.Response.StatusCode = 500;
            }

        }
    }
}