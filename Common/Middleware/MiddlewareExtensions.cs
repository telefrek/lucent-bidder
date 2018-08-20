using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        public static void UseLoadShedding(this IApplicationBuilder appBuilder)
            => appBuilder.UseMiddleware<LoadSheddingMiddleware>();

        public static void ConfigureLoadShedding(this IServiceCollection services, IConfiguration configuration)
            => services.Configure<LoadSheddingConfiguration>(configuration.GetSection("load_shedding"));
    }
}