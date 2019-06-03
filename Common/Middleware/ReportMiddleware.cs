using System;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Handle reporting API operations
    /// </summary>
    public class ReportMiddleware
    {
        ILogger<ReportMiddleware> _log;
        IBidLedger _ledger;
        ISerializationContext _serializationContext;

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="next">Ignored</param>
        /// <param name="logger">Logger for output</param>
        /// <param name="ledger">The ledger for the environment></param>
        /// <param name="serializationContext">Serializatoin context to use</param>
        public ReportMiddleware(RequestDelegate next, ILogger<ReportMiddleware> logger, IBidLedger ledger, ISerializationContext serializationContext)
        {
            _log = logger;
            _ledger = ledger;
            _serializationContext = serializationContext;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="httpContext">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var segments = httpContext.Request.Path.Value.Split(new char[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length == 0)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var ledgerId = segments.First();
                _log.LogInformation("Getting report for {0}", ledgerId);

                switch (httpContext.Request.Method.ToLowerInvariant())
                {
                    case "get":
                        // Query processing values
                        // var query = httpContext.Request.Query;
                        // var qp = new StringValues();

                        // Assume start/end for last hour
                        var start = DateTime.UtcNow.AddDays(-1);
                        start = start.AddTicks(-start.TimeOfDay.Ticks);
                        var end = DateTime.UtcNow.AddHours(1);
                        var numSegments = (int)end.Subtract(start).TotalHours;

                        // Read query parameters

                        var summaries = await _ledger.TryGetSummary(ledgerId, start, end, numSegments);
                        if (summaries.Count > 0)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status200OK;
                            await _serializationContext.WriteTo(summaries, httpContext.Response.Body, false, SerializationFormat.JSON);
                        }
                        else
                            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;

                        break;
                    default:
                        httpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                        break;
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to process request");
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}