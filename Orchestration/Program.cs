using Lucent.Common;
using Lucent.Common.Bootstrap;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


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
                    services.Configure<FormOptions>(options =>
                    {
                        options.MultipartBodyLengthLimit = long.MaxValue;
                        options.ValueLengthLimit = int.MaxValue;
                        options.MultipartHeadersLengthLimit = int.MaxValue;
                    });
                    services.AddLucentServices(hostingContext.Configuration, includeOrchestration: true);
                })
                .UseStartup<OrchestrationStartup>();
    }
}
