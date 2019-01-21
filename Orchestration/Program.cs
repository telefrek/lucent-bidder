using Lucent.Common.Bootstrap;
using Lucent.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

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
            .ConfigureServices(services =>
            {
                services.AddLucentServices(new ConfigurationBuilder().Build(), includeOrchestration: true);
            }).UseStartup<OrchestrationStartup>();
    }
}
