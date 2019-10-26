using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Caching;
using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Lucent.Common.Serialization;
using Microsoft.AspNetCore.Http;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Bootstrap
{
    /// <summary>
    /// 
    /// </summary>
    public class BidderStartup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;

            // Add metrics ftw  
            app.UseMetricServer();
            app.UseResponseCompression();
            app.UseMiddleware<MonitoringMiddleware>();
            app.ConfigureHealth();

            app.Map("/v1/stats", (a) =>
            {
                a.Run(async (ctx) =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "application/text";
                    var buffer = Encoding.UTF8.GetBytes(SourceCache.Scan());
                    await ctx.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                });
            });


            app.Map("/v1/sample", (a) =>
            {
                a.Run(async (HttpContext ctx) =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(BiddingMiddleware.LastRequest);
                    await ctx.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                });
            });

            app.Map("/v1/bidder", (a) =>
            {
                a.UseMiddleware<BiddingMiddleware>();
            });

            app.Map("/v1/postback", (a) =>
            {
                a.UseMiddleware<PostbackMiddleware>();
            });
        }
    }
}
