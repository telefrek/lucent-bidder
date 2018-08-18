using Lucent.Common.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    /// <summary>
    /// Messaging extensions
    /// </summary>
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Hook to configure messaging factory creation during runtime
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration) =>
            // Setup configuration
            services.Configure<RabbitConfiguration>(configuration.GetSection("rabbit"))
            // Only need a single message facory
            .AddSingleton<IMessageFactory, RabbitFactory>();
    }
}