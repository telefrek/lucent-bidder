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
            services.AddLucentServices(Configuration, includeOrchestration: true);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ConfigureHealth();
            app.Map("/api/campaigns", (a) =>
            {
                a.UseMiddleware<CampaignOrchestrator>();
            });
        }
    }
}
