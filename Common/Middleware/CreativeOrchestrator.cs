using System.IO;
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
    /// Handle Creative API management
    /// </summary>
    public class CreativeOrchestrator
    {
        readonly IStorageManager _storageManager;
        readonly ISerializationContext _serializationContext;
        readonly ILogger<CreativeOrchestrator> _logger;
        readonly IBasicStorageRepository<Creative> _creativeRepository;
        readonly IMessageFactory _messageFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        public CreativeOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<CreativeOrchestrator> logger)
        {
            _storageManager = storageManager;
            _creativeRepository = storageManager.GetBasicRepository<Creative>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;

            _messageFactory.CreateSubscriber<LucentMessage<Creative>>("entities", 0, "creative").OnReceive += UpdateCreatives;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creativeEvent"></param>
        /// <returns></returns>
        async Task UpdateCreatives(LucentMessage<Creative> creativeEvent)
        {
            if (creativeEvent.Body != null)
            {
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Creative,
                    EntityId = creativeEvent.Body.Id,
                };

                // This is awful, don't do this for real
                if (await _creativeRepository.TryUpdate(creativeEvent.Body))
                    evt.EventType = EventType.EntityUpdate;
                else if (await _creativeRepository.TryInsert(creativeEvent.Body))
                    evt.EventType = EventType.EntityAdd;
                else if (await _creativeRepository.TryRemove(creativeEvent.Body))
                    evt.EventType = EventType.EntityDelete;

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = "creative";
                    using (var ms = new MemoryStream())
                    {
                        await _serializationContext.WriteTo(evt, ms, true, SerializationFormat.JSON);
                        _logger.LogInformation("Sending {0}", Encoding.UTF8.GetString(ms.ToArray()));
                    }

                    await _messageFactory.CreatePublisher("bidding").TryPublish(msg);
                }
            }
        }


        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var c = await _serializationContext.ReadAs<Creative>(httpContext);
            if (c != null)
            {
                // Validate
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Creative,
                    EntityId = c.Id,
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "post":
                        if (await _creativeRepository.TryInsert(c))
                        {
                            httpContext.Response.StatusCode = 201;
                            evt.EventType = EventType.EntityAdd;
                            evt.EntityId = c.Id;
                            await _serializationContext.WriteTo(httpContext, c);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "put":
                    case "patch":
                        if (await _creativeRepository.TryUpdate(c))
                        {
                            httpContext.Response.StatusCode = 202;
                            evt.EventType = EventType.EntityUpdate;
                            await _serializationContext.WriteTo(httpContext, c);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "delete":
                        if (await _creativeRepository.TryRemove(c))
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
                    msg.Route = "creative";
                    await _messageFactory.CreatePublisher("bidding").TryPublish(msg);

                    var sync = _messageFactory.CreateMessage<LucentMessage<Creative>>();
                    sync.Body = c;
                    sync.Route = "creative";
                    await _messageFactory.CreatePublisher("entities").TryBroadcast(msg);
                }
            }
        }
    }
}