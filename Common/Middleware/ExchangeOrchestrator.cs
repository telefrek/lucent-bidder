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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Handle Exxchange API management
    /// </summary>
    public class ExchangeOrchestrator
    {
        readonly IStorageManager _storageManager;
        readonly ISerializationContext _serializationContext;
        readonly ILogger<ExchangeOrchestrator> _logger;
        readonly IStorageRepository<Exchange, Guid> _exchangeRepository;
        readonly IMessageFactory _messageFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="storageManager"></param>
        /// <param name="messageFactory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        public ExchangeOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<ExchangeOrchestrator> logger)
        {
            _storageManager = storageManager;
            _exchangeRepository = storageManager.GetRepository<Exchange, Guid>();
            _serializationContext = serializationContext;
            _messageFactory = messageFactory;
            _logger = logger;

            _messageFactory.CreateSubscriber<LucentMessage<Exchange>>("entities", 0, "exchange").OnReceive += UpdateExchanges;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignEvent"></param>
        /// <returns></returns>
        async Task UpdateExchanges(LucentMessage<Exchange> campaignEvent)
        {
            if (campaignEvent.Body != null)
            {
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Campaign,
                    EntityId = campaignEvent.Body.Id.ToString(),
                };

                // This is awful, don't do this for real
                if (await _exchangeRepository.TryUpdate(campaignEvent.Body))
                    evt.EventType = EventType.EntityUpdate;
                else if (await _exchangeRepository.TryInsert(campaignEvent.Body))
                    evt.EventType = EventType.EntityAdd;
                else if (await _exchangeRepository.TryRemove(campaignEvent.Body))
                    evt.EventType = EventType.EntityDelete;

                // Notify
                if (evt.EventType != EventType.Unknown)
                {
                    var msg = _messageFactory.CreateMessage<EntityEventMessage>();
                    msg.Body = evt;
                    msg.Route = "exchange";
                    using (var ms = new MemoryStream())
                    {
                        await _serializationContext.WriteTo(evt, ms, true, SerializationFormat.JSON);
                        _logger.LogInformation("Sending {0}", Encoding.UTF8.GetString(ms.ToArray()));
                    }

                    await _messageFactory.CreatePublisher("bidding").TryPublish(msg);
                }
            }
        }

        async Task<Exchange> ReadExchange(HttpContext httpContext)
        {
            Exchange exchange = null;

            if (httpContext.Request.ContentType.Contains("multipart"))
            {
                // /api/exchanges/id
                exchange = await _exchangeRepository.Get(Guid.Parse(httpContext.Request.Path.Value.Split("/").Last()));
                if (exchange != null)
                {
                    var header = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
                    var boundary = HeaderUtilities.RemoveQuotes(header.Boundary).Value;
                    var reader = new MultipartReader(boundary, httpContext.Request.Body);

                    var section = await reader.ReadNextSectionAsync();
                    if (section != null)
                    {
                        ContentDispositionHeaderValue contentDisposition;
                        if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition))
                        {
                            exchange.Code = new MemoryStream();
                            await httpContext.Request.Body.CopyToAsync(exchange.Code);
                            exchange.Code.Seek(0, SeekOrigin.Begin);
                            exchange.LastCodeUpdate = DateTime.UtcNow;
                        }
                    }
                }
            }
            else
            {
                exchange = await _serializationContext.ReadAs<Exchange>(httpContext);
            }

            return exchange;
        }


        /// <summary>
        /// Handle the call asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var exchange = await ReadExchange(httpContext);
            if (exchange != null)
            {
                // Validate
                var evt = new EntityEvent
                {
                    EntityType = EntityType.Exchange,
                    EntityId = exchange.Id.ToString(),
                };

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "post":
                        if (await _exchangeRepository.TryInsert(exchange))
                        {
                            httpContext.Response.StatusCode = 201;
                            evt.EventType = EventType.EntityAdd;
                            await _serializationContext.WriteTo(httpContext, exchange);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "put":
                    case "patch":
                        if (await _exchangeRepository.TryUpdate(exchange))
                        {
                            httpContext.Response.StatusCode = 202;
                            evt.EventType = EventType.EntityUpdate;
                            await _serializationContext.WriteTo(httpContext, exchange);
                        }
                        else
                            httpContext.Response.StatusCode = 409;
                        break;
                    case "delete":
                        if (await _exchangeRepository.TryRemove(exchange))
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
                    msg.Route = "exchange";
                    using (var ms = new MemoryStream())
                    {
                        await _serializationContext.WriteTo(evt, ms, true, SerializationFormat.JSON);
                        _logger.LogInformation("Sending {0}", Encoding.UTF8.GetString(ms.ToArray()));
                    }

                    await _messageFactory.CreatePublisher("bidding").TryPublish(msg);

                    var sync = _messageFactory.CreateMessage<LucentMessage<Exchange>>();
                    sync.Body = exchange;
                    sync.Route = "exchange";
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