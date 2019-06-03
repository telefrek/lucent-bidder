using System.IO;
using System.Linq;
using Lucent.Common.Entities;
using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Prometheus;

namespace Lucent.Common.Bootstrap
{
    /// <summary>
    /// 
    /// </summary>
    public class OrchestrationStartup
    {
        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="configuration"></param>
        public OrchestrationStartup(IConfiguration configuration) => Configuration = configuration;

        /// <summary>
        /// Startup config
        /// </summary>
        /// <value></value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMetricServer();
            app.UseMiddleware<MonitoringMiddleware>();
            app.ConfigureHealth();
            app.UseAuthentication();

            app.Map("/api/campaigns", (a) =>
            {
                a.UseMiddleware<EntityRestApi<Campaign>>();
            });

            app.Map("/api/creatives", (a) =>
            {
                a.UseMiddleware<CreativeApi>();
            });

            app.Map("/api/exchanges", (a) =>
            {
                a.UseMiddleware<ExchangeOrchestrator>();
            });

            app.Map("/api/filters", (a) =>
            {
                a.UseMiddleware<EntityRestApi<BidderFilter>>();
            });

            app.Map("/api/ledger", (a) =>
            {
                a.UseMiddleware<LedgerMiddleware>();
            });

            app.Map("/api/report", (a) =>
            {
                a.UseMiddleware<ReportMiddleware>();
            });

            app.Map("/api/budget/request", (a) =>
            {
                a.UseMiddleware<BudgetOrchestrator>();
            });

            var cachePeriod = 600;
            var path = Path.Combine(Directory.GetCurrentDirectory(), Configuration.GetValue("ContentPath", "adcontent"));
            Directory.CreateDirectory(path);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = "/creatives",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                }
            });
        }
    }
}
