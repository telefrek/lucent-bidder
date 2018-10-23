using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
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
        ISerializationContext _serializationContext;
        IExchangeRegistry _exchangeRegistry;
        IStorageManager _storageManager;
        List<Func<BidRequest, bool>> _bidFilters;

        int _next = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="log"></param>
        /// <param name="factory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="exchangeRegistry"></param>
        /// <param name="storageManager"></param>
        public BiddingMiddleware(RequestDelegate next, ILogger<BiddingMiddleware> log, IMessageFactory factory, ISerializationContext serializationContext, IExchangeRegistry exchangeRegistry, IStorageManager storageManager)
        {
            _log = log;
            _serializationContext = serializationContext;
            _exchangeRegistry = exchangeRegistry;
            _storageManager = storageManager;
            _bidFilters = _storageManager.GetRepository<BidderFilter>().Get().Result.Where(f => f.BidFilter != null).Select(f => f.BidFilter.GenerateCode()).ToList();

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
                context.Response.StatusCode = 204;
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

                    if (request != null && !_bidFilters.Any(f => f.Invoke(request)))
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