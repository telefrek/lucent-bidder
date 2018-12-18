using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Entities;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lucent.Orchestration
{
    /// <summary>
    /// Handle Campaign API management
    /// </summary>
    public class CampaignOrchestrator
    {
        readonly IStorageManager _storageManager;
        readonly ISerializationContext _serializationContext;
        readonly ILogger<CampaignOrchestrator> _logger;
        readonly IBasicStorageRepository<Campaign> _campaignRepository;
        readonly IMessageFactory _messageFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="storageManager"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        public CampaignOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<CampaignOrchestrator> logger)
        {
            _storageManager = storageManager;
            _campaignRepository = storageManager.GetBasicRepository<Campaign>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;
        }

        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var c = await _serializationContext.ReadAs<Campaign>(httpContext);
            if (c != null)
            {
                // Validate
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Campaign,
                    EntityId = c.Id,
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "post":
                        if (await _campaignRepository.TryInsert(c))
                        {
                            evt.EventType = EventType.EntityAdd;
                            evt.EntityId = c.Id;
                            await _serializationContext.WriteTo(httpContext, c);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "put":
                    case "patch":
                        if (await _campaignRepository.TryUpdate(c))
                        {
                            evt.EventType = EventType.EntityUpdate;
                            await _serializationContext.WriteTo(httpContext, c);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "delete":
                        if (await _campaignRepository.TryRemove(c))
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
                    var msg = _messageFactory.CreateMessage<LucentMessage<EntityEvent>>();
                    msg.Body = evt;
                    msg.Route = "campaign";
                    using (var ms = new MemoryStream())
                    {
                        await _serializationContext.WriteTo(evt, ms, true, SerializationFormat.JSON);
                        _logger.LogInformation("Sending {0}", Encoding.UTF8.GetString(ms.ToArray()));
                    }
                    
                    await _messageFactory.CreatePublisher("entities").TryBroadcast(msg);
                }
            }
        }
    }
}