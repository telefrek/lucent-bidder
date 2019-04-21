using System.Threading;
using Lucent.Common;
using Lucent.Common.Bootstrap;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bidder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(16, 2048);
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
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
                    services.AddLucentServices(hostingContext.Configuration, includeBidder: true);
                })
                .UseStartup<BidderStartup>();
    }
}
