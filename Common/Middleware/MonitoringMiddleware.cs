using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Monitor the call times
    /// </summary>
    public class MonitoringMiddleware
    {
        ILogger<MonitoringMiddleware> _log;
        RequestDelegate _nextHandler;

        Histogram _apiLatency = Metrics.CreateHistogram("api_latency", "Latency for each api call", new HistogramConfiguration
        {
            LabelNames = new string[] { "method", "path" },
            Buckets = new double[] { 0.005, 0.010, 0.015, 0.025, 0.050, 0.075, 0.100, 0.125, 0.150, 0.200, 0.25, 0.5, 0.75, 1.0 },
        });

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="log"></param>
        public MonitoringMiddleware(RequestDelegate next, ILogger<MonitoringMiddleware> log)
        {
            _log = log;
            _nextHandler = next;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="httpContext">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            using (_apiLatency.CreateContext(httpContext.Request.Method, string.Join('/', httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Take(2).ToArray())))
                try
                {
                    await _nextHandler(httpContext);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Exception bled into monitoring!");
                    httpContext.Response.StatusCode = httpContext.Request.Path.Value.Contains("/bidder") ? 204 : 503; // safe failure mode
                }
        }
    }
}