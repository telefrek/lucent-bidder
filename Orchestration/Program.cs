using System.Text;
using Lucent.Common;
using Lucent.Common.Bootstrap;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Orchestration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(kestrelOptions =>
                {
                    kestrelOptions.Limits.MaxRequestBodySize = null;
                })
                .UseSockets(socketTransportOptions =>
                {
                    socketTransportOptions.IOQueueCount = 16;
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddCors(options =>
                    {
                        options.AddPolicy("localhostPolicy",
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .WithExposedHeaders("X-LUCENT-ETAG")
                                .AllowAnyMethod()
                                .AllowCredentials();
                        });
                    });
                    services.Configure<FormOptions>(options =>
                    {
                        options.MultipartBodyLengthLimit = long.MaxValue;
                        options.ValueLengthLimit = int.MaxValue;
                        options.MultipartHeadersLengthLimit = int.MaxValue;
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
                    services.AddLucentServices(hostingContext.Configuration, includeOrchestration: true);
                })
                .UseStartup<OrchestrationStartup>();
    }
}
