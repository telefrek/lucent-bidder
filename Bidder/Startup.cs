using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            services.ConfigureLoadShedding(Configuration);
            services.AddMessaging(Configuration);
            services.AddSerialization(Configuration);
            services.AddOpenRTBSerializers();
            services.AddSingleton<IBidHandler, BidHandler>();
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

            //app.UseHttpsRedirection(); turn this off for now
            app.UseCookiePolicy();
            app.UseLoadShedding();

            // Add metrics ftw
            app.UseMetricServer();

            // Track the api latency for each request type
            var api_latency = Metrics.CreateHistogram("bidder_latency", "Latency for each bidder call", new HistogramConfiguration
            {
                LabelNames = new string[] { "method", "path" },
                Buckets = new double[] { 0.005, 0.010, 0.015, 0.025, 0.050, 0.075, 0.100, 0.125, 0.150, 0.200, 0.25, 0.5, 0.75, 1.0 },
            });

            // This should be fun...
            app.Use(async (context, next) =>
            {
                var instance = api_latency.WithLabels(context.Request.Method, context.Request.Path);
                var sw = Stopwatch.StartNew();
                await next().ContinueWith(t =>
                {
                    instance.Observe(sw.ElapsedTicks * 1000d / Stopwatch.Frequency);
                });
            });

            var routeBuilder = new RouteBuilder(app);
            routeBuilder.MapPost("/v1/bidder", async (context) =>
            {
                await context.RequestServices.GetRequiredService<IBidHandler>().HandleAsync(context);
            });

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
        ISerializationContext _serializationContext;
        int _next = 0;

        public BidHandler(ILogger<BidHandler> log, IMessageFactory factory, ISerializationContext serializationContext)
        {
            _log = log;
            _serializationContext = serializationContext;
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
            using (var serializationReader = _serializationContext.CreateReader(context.Request.Body, false, SerializationFormat.JSON))
            {
                if (await serializationReader.HasNextAsync())
                {
                    var request = await serializationReader.ReadAsAsync<BidRequest>();

                    if (request != null)
                        _log.LogInformation("Got request {0}", request.Id);

                    context.Response.StatusCode = request == null ? 400 : 204;
                }
                else context.Response.StatusCode = 400;
            }
        }
    }
}
