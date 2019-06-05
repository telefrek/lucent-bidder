using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

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

        class HourlyReport
        {
            public string Hour { get; set; }
            public int Wins { get; set; }
            public int Conversions { get; set; }
            public double Amount { get; set; }
            public double eCPM { get; set; }
            public double eCPA { get; set; }
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
                        var query = httpContext.Request.Query;
                        var qp = new StringValues();

                        // Assume start/end for last hour
                        var start = DateTime.UtcNow.AddHours(-1);
                        var end = DateTime.UtcNow;
                        var offset = DateTime.Now.Hour - DateTime.UtcNow.Hour;

                        if (query.ContainsKey("offset") && query.TryGetValue("offset", out qp))
                            offset = int.Parse(qp);

                        // Read query parameters
                        if (query.ContainsKey("start") && query.TryGetValue("start", out qp))
                            start = DateTime.Parse(qp).ToUniversalTime();
                        else
                        {
                            start = DateTime.Now.AddDays(-1);
                            start = start.AddHours(-1 * start.Hour);
                        }
                        start = start.AddHours(offset);

                        if (query.ContainsKey("end") && query.TryGetValue("end", out qp))
                        {
                            end = DateTime.Parse(qp).ToUniversalTime();
                            end = end.AddHours(offset);
                        }
                        else
                            end = start.AddDays(1);

                        var summaries = new List<HourlyReport>();
                        while (start < end)
                        {
                            foreach (var summary in await _ledger.TryGetSummary(ledgerId, start, start.AddHours(1), null))
                            {
                                var report = new HourlyReport()
                                {
                                    Amount = summary.Amount,
                                    Wins = summary.Bids,
                                    Hour = start.AddHours(-offset).ToString(),
                                };
                                if (report.Wins > 0)
                                    report.eCPM = report.Amount * 1000d / report.Wins;

                                summaries.Add(report);
                            }
                            start = start.AddHours(1);
                        }

                        if (summaries.Count > 0)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status200OK;
                            httpContext.Response.ContentType = "application/json";
                            var body = JsonConvert.SerializeObject(new { Id = ledgerId, Hourly = summaries });
                            await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(body), 0, Encoding.UTF8.GetByteCount(body));
                            // await _serializationContext.WriteTo(summaries, httpContext.Response.Body, false, SerializationFormat.JSON);
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