using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Messaging;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace Bidder
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) => services.AddLucentServices(Configuration, includeBidder: true);

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
        }
    }
}
