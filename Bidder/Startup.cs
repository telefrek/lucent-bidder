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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddLucentServices(Configuration, localOnly: true, includeBidder: true);
            services.ConfigureLoadShedding(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
            ThreadPool.SetMinThreads(64, 64);
            

            //app.UseHttpsRedirection(); turn this off for now
            app.UseCookiePolicy();

            // Add metrics ftw
            app.UseMetricServer();
            app.UseMiddleware<MonitoringMiddleware>();
            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapGet("/health", (context) =>
            {
                context.Response.StatusCode = 200;
                return Task.CompletedTask;
            });

            routeBuilder.MapGet("/ready", (context) =>
            {
                context.Response.StatusCode = 200;
                return Task.CompletedTask;
            });

            app.UseRouter(routeBuilder.Build());
            //app.UseLoadShedding();
            app.UseMiddleware<BiddingMiddleware>();
        }
    }
}
