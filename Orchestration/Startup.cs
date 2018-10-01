using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Orchestration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Orchestration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.ConfigureLoadShedding(Configuration);
            services.AddMessaging(Configuration);
            services.AddSerialization(Configuration);
            services.AddOpenRTBSerializers();
            services.AddSingleton<ICampaignProcessor, CampaignManager>();            
            services.AddStorage(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookiePolicy();
            app.UseLoadShedding();

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

            app.ApplicationServices.GetService<ICampaignProcessor>().Start();
        }
    }
}
