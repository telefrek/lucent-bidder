using System;
using System.Text;
using Lucent.Common.Bootstrap;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Lucent.Common.Test
{
    public class LucentTestWebHost<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
    {
        IConfiguration _config;

        public IServiceProvider Provider { get; set; }

        public Action<IServiceCollection> UpdateServices { get; set; } = (sp) => { };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logBuilder =>
            {
                logBuilder.AddConsole();
            })

            .UseStartup(typeof(TStartup))
            .UseSockets()
            .ConfigureServices(services =>
            {
                if (typeof(TStartup) == typeof(OrchestrationStartup))
                {

                    services.AddCors(options =>
                    {
                        options.AddPolicy("localhostPolicy",
                        bld =>
                        {
                            bld.AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                            bld.SetIsOriginAllowed((o) => true);
                        });
                    });

                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,

                                ValidIssuer = "https://lucentbid.com",
                                ValidAudience = "https://lucentbid.com",
                                IssuerSigningKey = new JwtTokenGenerator().GetKey(),
                            };
                        });
                }
                // Add ALL the services muhahahaha
                services.AddLucentServices(new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile("testsettings.json", true, true)
                        .Build(), true, true, true, true, true);

                UpdateServices(services);

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