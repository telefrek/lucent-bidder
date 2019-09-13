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
            public Dictionary<string, int> Details { get; set; } = new Dictionary<string, int>();
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
                        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
                        var end = start.AddHours(24);
                        var offset = 0;
                        var format = "csv";
                        var detailed = false;

                        if (query.ContainsKey("offset") && query.TryGetValue("offset", out qp))
                            offset = int.Parse(qp);

                        if (query.ContainsKey("detailed") && query.TryGetValue("detailed", out qp))
                            detailed = bool.Parse(qp);

                        if (query.ContainsKey("format") && query.TryGetValue("format", out qp))
                            format = qp;

                        if (query.ContainsKey("day") && query.TryGetValue("day", out qp))
                        {
                            var dt = DateTime.Parse(qp).ToUniversalTime();
                            start = new DateTime(dt.Year, dt.Month, dt.Day).AddHours(offset);
                            end = start.AddHours(24);
                        }

                        _log.LogInformation("Report start {0} to {1}", start, end);

                        var summaries = new List<HourlyReport>();
                        var total = 0d;
                        var bids = 0d;
                        while (start < end)
                        {
                            foreach (var summary in await _ledger.TryGetSummary(ledgerId, start, start.AddHours(1), null, detailed))
                            {
                                var report = new HourlyReport()
                                {
                                    Amount = summary.Amount,
                                    Wins = summary.Bids,
                                    Hour = start.AddHours(-offset).ToString(),
                                    Details = summary.Metadata ?? new Dictionary<string, int>(),
                                };
                                total += summary.Amount;
                                bids += summary.Bids;
                                if (report.Wins > 0)
                                    report.eCPM = report.Amount * 1000d / report.Wins;

                                summaries.Add(report);
                            }
                            start = start.AddHours(1);
                        }

                        if (summaries.Count > 0)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status200OK;

                            switch (format)
                            {
                                case "csv":
                                    httpContext.Response.ContentType = "text/csv";
                                    var columns = new HashSet<string>();
                                    foreach (var entry in summaries)
                                        foreach (var key in entry.Details.Keys)
                                            columns.Add(key);
                                    var sb = new StringBuilder();
                                    sb.AppendLine("Entity:\t{0}".FormatWith(ledgerId));
                                    sb.AppendLine("Total:\t{0}".FormatWith(total));
                                    sb.AppendLine("Wins:\t{0}".FormatWith(bids));
                                    sb.AppendLine("");
                                    sb.AppendLine("Hour\tAmount\tWins\teCPM{0}".FormatWith(columns.Count > 0 ? "\t" + string.Join("\t", columns.ToArray()) : ""));

                                    foreach (var entry in summaries)
                                    {
                                        sb.Append("{0}\t{1}\t{2}\t{3}".FormatWith(DateTime.Parse(entry.Hour).Hour, entry.Amount, entry.Wins, entry.eCPM));
                                        foreach(var key in columns.ToArray())
                                            sb.Append("\t{0}".FormatWith(entry.Details.GetValueOrDefault(key, 0)));
                                        sb.AppendLine();
                                    }
                                    await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(sb.ToString()), 0, sb.Length);
                                    break;
                                case "json":
                                default:
                                    httpContext.Response.ContentType = "application/json";
                                    var body = JsonConvert.SerializeObject(new { Id = ledgerId, Total = new { Amount = total, Bids = bids }, Hourly = summaries });
                                    await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(body), 0, Encoding.UTF8.GetByteCount(body));
                                    break;
                            }
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