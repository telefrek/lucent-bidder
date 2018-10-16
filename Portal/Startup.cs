using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Portal.Data;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telefrek.LDAP;
using Lucent.Portal.Hubs;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Newtonsoft.Json;
using Lucent.Common;
using Lucent.Common.Messaging;
using Newtonsoft.Json.Linq;
using Prometheus;

namespace Portal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        IMessageSubscriber<LucentMessage> _sub;

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

            services.AddMediaScanner(Configuration);
            services.AddDbContext<PortalDbContext>(options =>
                              options.UseInMemoryDatabase("local"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvc().AddRazorOptions(options =>
            {
                options.PageViewLocationFormats.Add("/Pages/Partials/{0}.cshtml");
            });            
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddScoped<ICampaignUpdateContext, CampaignUpdateContext>();

            // Add the ldap auth
            services.AddLDAPAuth(Configuration);
            services.AddMessaging(Configuration);
            services.AddSignalR();
            services.AddRouting();
            services.AddSerialization(Configuration);
            services.AddOpenRTBSerializers();
            services.AddStorage(Configuration);
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

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseLDAPAuth();
            
            app.UseMetricServer();
            app.UseApiLatency();

            app.UseMvc();
            app.UseSignalR(routes =>
            {
                routes.MapHub<CampaignHub>("/campaignHub");
            });

            _sub = app.ApplicationServices.GetRequiredService<IMessageFactory>().CreateSubscriber<LucentMessage>("campaigns", 0);
            _sub.OnReceive = (m) =>
            {
                if (m != null)
                {
                    dynamic obj = JObject.Parse(m.Body);
                    var id = Guid.Parse((string)obj.id);
                    var amt = (double)obj.amount;

                    app.ApplicationServices.CreateScope().ServiceProvider.GetService<ICampaignUpdateContext>().UpdateCampaignSpendAsync(id, amt, CancellationToken.None).Wait();
                }
            };
        }
    }
}
