using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for controlling bids
    /// </summary>
    public class BiddingMiddleware
    {
        ILogger<BiddingMiddleware> _log;
        IMessageSubscriber<LucentMessage> _subscriber;
        ISerializationContext _serializationContext;
        IExchangeRegistry _exchangeRegistry;
        int _next = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="log"></param>
        /// <param name="factory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="exchangeRegistry"></param>
        public BiddingMiddleware(RequestDelegate next, ILogger<BiddingMiddleware> log, IMessageFactory factory, ISerializationContext serializationContext, IExchangeRegistry exchangeRegistry)
        {
            _log = log;
            _serializationContext = serializationContext;
            _exchangeRegistry = exchangeRegistry;
            _subscriber = factory.CreateSubscriber<LucentMessage>("campaigns", 0);
            _subscriber.OnReceive = (m) =>
           {
               if (m != null)
               {
                   _log.LogInformation("Received message: {0}", m.Body);
                   Interlocked.Exchange(ref _next, 1);
               }
           };
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task HandleAsync(HttpContext context)
        {
            // Validate we can find a matching exchange
            var exchange = _exchangeRegistry.Exchanges.FirstOrDefault(e => e.IsMatch(context));
            if (exchange == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var format = (context.Request.ContentType ?? "").Contains("protobuf") ? SerializationFormat.PROTOBUF : SerializationFormat.JSON;

            var encoding = StringValues.Empty;
            if (context.Request.Headers.TryGetValue("Content-Encoding", out encoding))
                if (encoding.Any(e => e.Contains("gzip")))
                    format |= SerializationFormat.COMPRESSED;

            using (var serializationReader = _serializationContext.CreateReader(context.Request.Body, false, format))
            {
                if (await serializationReader.HasNextAsync())
                {
                    var request = await serializationReader.ReadAsAsync<BidRequest>();

                    if (request != null)
                    {
                        var response = await exchange.Bid(request);
                        if (response != null && (response.Bids ?? new SeatBid[0]).Length > 0)
                        {
                            using (var serializationWriter = _serializationContext.CreateWriter(context.Response.Body, true, format))
                                await serializationWriter.WriteAsync(response);
                            context.Response.StatusCode = 200;
                        }
                        else
                            context.Response.StatusCode = 204;
                    }
                    else
                        context.Response.StatusCode = request == null ? 400 : 204;
                }
                else context.Response.StatusCode = 400;
            }
        }
    }
}