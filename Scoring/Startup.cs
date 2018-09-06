using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TensorFlow;

namespace Scoring
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test" } },
                User = new User { Gender = Gender.Male }
            };

            var bFilter = new BidFilter
            {
                ImpressionFilters = new[] { new Filter<Impression> { Property = "BidCurrency", Value = "CAN" } },
                UserFilters = new[] { new Filter<User> { Property = "Gender", Value = Gender.Unknown } },
                GeoFilters = new[] { new Filter<Geo> { Property = "Country", Value = "CAN" } }
            };

            var f = bFilter.GenerateCode();

            Console.WriteLine("Warmup");
            var status = false;
            for (var i = 0; i < 100; ++i)
                status = f.Invoke(req);

            Console.WriteLine("Starting");
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 10000; ++i)
                status |= f.Invoke(req);
            sw.Stop();
            var ns = sw.ElapsedTicks * 1000000000.0 / Stopwatch.Frequency;
            Console.WriteLine("Total Time {0:#,##0.000} ns", ns);
            Console.WriteLine("{0:#,##0.000} ns/op", ns / 10000);


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
