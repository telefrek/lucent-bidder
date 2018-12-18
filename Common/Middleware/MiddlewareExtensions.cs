using System.Threading.Tasks;
using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Inform the ASP infra to use the load shedding middleware
        /// </summary>
        /// <param name="appBuilder"></param>
        public static void UseLoadShedding(this IApplicationBuilder appBuilder)
            => appBuilder.UseMiddleware<LoadSheddingMiddleware>();

        /// <summary>
        /// Configure load shedding middleware
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureLoadShedding(this IServiceCollection services, IConfiguration configuration)
            => services.Configure<LoadSheddingConfiguration>(configuration.GetSection("load_shedding"));

        /// <summary>
        /// Configure basic healthchecks
        /// </summary>
        /// <param name="applicationBuilder"></param>
        public static void ConfigureHealth(this IApplicationBuilder applicationBuilder)
        {
            var routeBuilder = new RouteBuilder(applicationBuilder);

            routeBuilder.MapGet("/health", async (context) =>
            {
                context.Response.StatusCode = 200;
                await Task.CompletedTask;
            });

            routeBuilder.MapGet("/ready", async (context) =>
            {
                context.Response.StatusCode = 200;
                await Task.CompletedTask;
            });

            applicationBuilder.UseRouter(routeBuilder.Build());
        }
    }
}