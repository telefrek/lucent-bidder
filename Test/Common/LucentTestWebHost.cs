using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Test
{
    public class LucentTestWebHost<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
    {
        IConfiguration _config;

        public IServiceProvider Provider { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logBuilder =>
            {
                logBuilder.AddConsole();
            });

            builder.UseStartup(typeof(TStartup));

            builder.ConfigureServices(services =>
            {
                // Add ALL the services muhahahaha
                services.AddLucentServices(new ConfigurationBuilder().AddEnvironmentVariables().AddJsonFile("appsettings.json").Build(), true, true, true, true, true);

                // Build the service provider.
                Provider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (ApplicationDbContext).
                using (var scope = Provider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    // Configure extra junk here
                }
            });
        }
    }
}