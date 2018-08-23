using Lucent.Common.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {

        /// <summary>
        /// Hook to configure messaging factory creation during runtime
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration) =>
            // Setup configuration
            services.Configure<CassandraConfiguration>(configuration.GetSection("cassandra"))
            // Only need a single message facory
            .AddSingleton<IStorageManager, CassandraStorageManager>();
    }
}