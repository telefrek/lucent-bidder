using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Lucent.Common.Bidding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lucent.Common.Exchanges;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Serializers;
using Lucent.Common.Filters;
using Lucent.Common.Filters.Serializers;
using Lucent.Common.Media;

namespace Lucent.Common
{
    /// <summary>
    /// Dependency Injection extensions
    /// </summary>
    public static partial class LucentDIExtensions
    {
        /// <summary>
        /// Sets up the bidder
        /// </summary>
        /// <param name="services">The current services collection</param>
        /// <param name="configuration">The current configuration</param>
        /// <param name="localOnly">Indicate if this is a setup for local resources only</param>
        /// <param name="includePortal">Flag for portal servicees</param>
        /// <param name="includeBidder">Flag for bidder services</param>
        /// <param name="includeOrchestration">Flag for orchestartion services</param>
        /// <param name="includeScoring">Flag for scoring services</param>
        /// <returns>A modified set of services</returns>
        public static IServiceCollection AddLucentServices(this IServiceCollection services, IConfiguration configuration, bool localOnly = false, bool includePortal = false, bool includeBidder = false, bool includeOrchestration = false, bool includeScoring = false)
        {
            // Add serialization
            services.AddSingleton<ISerializationRegistry, SerializationRegistry>()
                .AddSingleton<ISerializationContext, LucentSerializationContext>() 
                .AddOpenRTBSerializers();
            
            var registry = services.BuildServiceProvider().GetRequiredService<ISerializationRegistry>();
            if (!registry.IsSerializerRegisterred<Campaign>())
            {
                registry.Register<Campaign>(new CampaignSerializer());
                registry.Register<CampaignSchedule>(new CampaignScheduleSerializer());
                registry.Register<Creative>(new CreativeSearializer());
                registry.Register<CreativeContent>(new CreativeContentSerializer());
                registry.Register<Filter>(new FilterSerializer());
                registry.Register<BidFilter>(new BidFilterSerializer());
            }

            // Setup storage, messaging options for local vs distributed cluster
            if (localOnly)
            {
                services.AddSingleton<IStorageManager, InMemoryStorage>();
                services.AddSingleton<IMessageFactory, InMemoryMessageFactory>();
            }
            else
            {
                // Setup storage
                services.Configure<CassandraConfiguration>(configuration.GetSection("cassandra"))
                    .AddSingleton<IStorageManager, CassandraStorageManager>();

                services.Configure<RabbitConfiguration>(configuration.GetSection("rabbit"))
                    .AddSingleton<IMessageFactory, RabbitFactory>();
            }

            if (includePortal)
            {
                services.Configure<MediaConfig>(configuration.GetSection("media"))
                    .AddSingleton<IMediaScanner, MediaScanner>();
            }

            if (includeBidder)
            {
                services.AddSingleton<IBidFactory, BidFactory>()
                    .AddSingleton<IExchangeRegistry, ExchangeRegistry>();
            }

            if (includeOrchestration)
            {

            }

            if (includeScoring)
            {

            }

            return services;
        }
    }
}