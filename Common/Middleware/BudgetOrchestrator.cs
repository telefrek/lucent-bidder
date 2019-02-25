using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Budget;
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
    /// Handle Campaign API management
    /// </summary>
    public class BudgetOrchestrator
    {
        readonly IStorageManager _storageManager;
        readonly ISerializationContext _serializationContext;
        readonly ILogger<BudgetOrchestrator> _logger;
        readonly IStorageRepository<Campaign> _campaignRepository;
        readonly IMessageFactory _messageFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        public BudgetOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<BudgetOrchestrator> logger)
        {
            _storageManager = storageManager;
            _campaignRepository = storageManager.GetRepository<Campaign>();
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
            _logger.LogInformation("Reading request");
            var request = await _serializationContext.ReadAs<BudgetRequest>(httpContext);

            if (request != null)
            {
                _logger.LogInformation("Request : {0}", request.CorrelationId);
                
                // Validate
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Unknown,
                    EntityId = request.EntityId,
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "post":
                        // Add budget
                        var bmsg = _messageFactory.CreateMessage<BudgetEventMessage>();
                        bmsg.Body = new BudgetEvent { EntityId = request.EntityId, Amount = request.Amount, CorrelationId = request.CorrelationId };
                        bmsg.Route = "event_test";
                        if (await _messageFactory.CreatePublisher(Topics.BUDGET).TryPublish(bmsg))
                            httpContext.Response.StatusCode = 202;
                        else
                            httpContext.Response.StatusCode = 500;
                        break;
                    case "get":
                    case "put":
                    case "patch":
                    case "delete":
                        httpContext.Response.StatusCode = 405;
                        break;
                }

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = "campaign";
                    await _messageFactory.CreatePublisher(Topics.BIDDING).TryPublish(msg);

                    var sync = _messageFactory.CreateMessage<LucentMessage<BudgetRequest>>();
                    sync.Body = request;
                    sync.Route = "campaign";
                    await _messageFactory.CreatePublisher(Topics.ENTITIES).TryBroadcast(msg);
                }
            }
        }
    }
}