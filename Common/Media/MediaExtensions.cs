using Lucent.Common.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Adds the media scanner classes
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddMediaScanner(this IServiceCollection provider, IConfiguration configuration)
        {
            provider.AddSingleton<IMediaScanner, MediaScanner>();
            provider.Configure<MediaConfig>(configuration.GetSection("media"));
            return provider;
        }
    }
}