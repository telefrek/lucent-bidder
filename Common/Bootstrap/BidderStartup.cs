using System.Threading;
using Lucent.Common;
using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
            ThreadPool.SetMinThreads(64, 64);

            // Add metrics ftw
            app.UseMetricServer();
            app.UseMiddleware<MonitoringMiddleware>();
            app.ConfigureHealth();

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
