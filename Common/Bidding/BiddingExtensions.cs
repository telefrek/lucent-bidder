using Lucent.Common.Bidding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Add bidding functionality
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBidding(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBidFactory, BidFactory>();
            return services;
        }
    }
}