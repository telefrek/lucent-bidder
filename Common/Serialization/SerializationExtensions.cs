using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Serialization extension methods
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Hook to configure serialization registry creation during runtime
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddSerialization(this IServiceCollection services, IConfiguration configuration) =>
            // Each request should get it's own seriailzation registry, depending on context
            services.AddScoped<ISerializationRegistry, SerializationRegistry>();
    }
}