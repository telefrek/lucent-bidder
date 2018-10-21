using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
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
    }
}