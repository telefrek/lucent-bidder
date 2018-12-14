using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Newtonsoft.Json;
using Prometheus;

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
        RequestDelegate _nextHandler;
        Histogram _serializerTiming = Metrics.CreateHistogram("serializer_latency", "Latency for each bidder call", new HistogramConfiguration
        {
            LabelNames = new string[] { "protocol", "direction" },
            Buckets = new double[] { 0.001, 0.002, 0.005, 0.007, 0.01, 0.015 },
        });

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
            _bidFilters = _storageManager.GetRepository<BidderFilter, string>().GetAll().Result.Where(f => f.BidFilter != null).Select(f => f.BidFilter.GenerateCode()).ToList();
            _nextHandler = next;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="httpContext">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.StartsWithSegments("/v1/bidder"))
            {
                await _nextHandler.Invoke(httpContext);
                return;
            }
            // else
            // {
            //     var instance = _serializerTiming.WithLabels("JSON", "deserialize");
            //     var sw = Stopwatch.StartNew();
            //     var request = JsonConvert.DeserializeObject(await new StreamReader(httpContext.Request.Body).ReadToEndAsync());
            //     instance.Observe(sw.ElapsedTicks * 1d / Stopwatch.Frequency);
            //     httpContext.Response.StatusCode = 204;
            //     return;
            // }

            // Wrap the body to force the contents to flush
            using (var bodyContents = httpContext.Request.Body)
            {
                var format = (httpContext.Request.ContentType ?? "").Contains("protobuf") ? SerializationFormat.PROTOBUF : SerializationFormat.JSON;

                var encoding = StringValues.Empty;
                if (httpContext.Request.Headers.TryGetValue("Content-Encoding", out encoding))
                    if (encoding.Any(e => e.Contains("gzip")))
                        format |= SerializationFormat.COMPRESSED;

                var instance = _serializerTiming.WithLabels(format.ToString(), "deserialize");
                var sw = Stopwatch.StartNew();
                var request = await _serializationContext.ReadFrom<BidRequest>(bodyContents, true, format);
                instance.Observe(sw.ElapsedTicks * 1d / Stopwatch.Frequency);

                if (request != null && !_bidFilters.Any(f => f.Invoke(request)))
                {
                    // Validate we can find a matching exchange
                    var exchange = _exchangeRegistry.Exchanges.FirstOrDefault(e => e.IsMatch(httpContext));
                    if (exchange == null)
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                        return;
                    }

                    httpContext.Items.Add("exchange", exchange);
                    var response = await exchange.Bid(request, httpContext);

                    // Clear the compression flag and re-validate the accept header
                    format &= ~SerializationFormat.COMPRESSED;
                    if (httpContext.Request.Headers.TryGetValue("Accept-Encoding", out encoding))
                        if (encoding.Any(e => e.Contains("gzip")))
                            format |= SerializationFormat.COMPRESSED;

                    if (response != null && (response.Bids ?? new SeatBid[0]).Length > 0)
                    {
                        await _serializationContext.WriteTo(response, httpContext.Response.Body, true, format);
                        httpContext.Response.StatusCode = StatusCodes.Status200OK;
                    }
                    else
                        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                }
                else
                    httpContext.Response.StatusCode = request == null ? StatusCodes.Status400BadRequest : StatusCodes.Status204NoContent;
            }

        }
    }
}