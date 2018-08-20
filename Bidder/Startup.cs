using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Messaging;
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
            services.ConfigureLoadShedding(Configuration);

            services.AddMessaging(Configuration);
            services.AddSingleton<ISerializationRegistry, SerializationRegistry>();
            services.AddSingleton<IBidHandler, BidHandler>();
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

            //app.UseHttpsRedirection(); turn this off for now
            app.UseLoadShedding();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            var routeBuilder = new RouteBuilder(app);
            app.UseMvc();
            routeBuilder.MapPost("/v1/bidder", async (context) =>
            {
                await context.RequestServices.GetRequiredService<IBidHandler>().HandleAsync(context);
            });

            app.UseRouter(routeBuilder.Build());
        }
    }

    public interface IBidHandler
    {
        Task HandleAsync(HttpContext context);
    }

    public class BidHandler : IBidHandler
    {
        ILogger<BidHandler> _log;
        IMessageSubscriber<LucentMessage> _subscriber;
        int _next = 0;

        public BidHandler(ILogger<BidHandler> log, IMessageFactory factory)
        {
            _log = log;
            _subscriber = factory.CreateSubscriber<LucentMessage>("campaigns", 0);
            _subscriber.OnReceive = (m) =>
           {
               if (m != null)
               {
                   _log.LogInformation("Received message: {0}", m.Body);
                   Interlocked.Exchange(ref _next, 1);
               }
           };
        }

        public async Task HandleAsync(HttpContext context)
        {
            var sstream = context.Request.Body.WrapSerializer(context.RequestServices, SerializationFormat.JSON, false);

            using (var reader = sstream.Reader)
            {
                if (await reader.HasNextAsync())
                {
                    var bidRequest = reader.ReadAsAsync<BidRequest>();

                    if (bidRequest != null)
                    {
                        if (Interlocked.Exchange(ref _next, 0) > 0)
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            var resp = new BidResponse { Id = "hello", CorrelationId = "world" };
                            using (var serial = context.Response.Body.WrapSerializer(context.RequestServices, SerializationFormat.JSON, false).Writer)
                            {
                                await serial.WriteAsync(resp);
                                await serial.FlushAsync();
                            }
                        }
                        else
                            context.Response.StatusCode = 204;

                        await Task.Delay(50);
                        return;
                    }
                }
            }

            context.Response.StatusCode = 403;
        }
    }
}
