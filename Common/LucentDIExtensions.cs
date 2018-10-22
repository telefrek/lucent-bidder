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
using Lucent.Common.OpenRTB;
using Lucent.Common.OpenRTB.Serializers;

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
                .AddSingleton<ISerializationContext, LucentSerializationContext>();

            var registry = services.BuildServiceProvider().GetRequiredService<ISerializationRegistry>();
            if (!registry.IsSerializerRegisterred<Campaign>())
            {
                registry.Register<Campaign>(new CampaignSerializer());
                registry.Register<CampaignSchedule>(new CampaignScheduleSerializer());
                registry.Register<Creative>(new CreativeSearializer());
                registry.Register<CreativeContent>(new CreativeContentSerializer());
                registry.Register<Filter>(new FilterSerializer());
                registry.Register<BidFilter>(new BidFilterSerializer());

                // Register the enums
                registry.Register<AdPosition>(new EnumEntitySerializer<AdPosition>());
                registry.Register<StartDelay>(new EnumEntitySerializer<StartDelay>());
                registry.Register<AuctionType>(new EnumEntitySerializer<AuctionType>());
                registry.Register<ISP>(new EnumEntitySerializer<ISP>());
                registry.Register<GeoType>(new EnumEntitySerializer<GeoType>());
                registry.Register<Context>(new EnumEntitySerializer<Context>());
                registry.Register<ConnectionType>(new EnumEntitySerializer<ConnectionType>());
                registry.Register<DeviceType>(new EnumEntitySerializer<DeviceType>());
                registry.Register<VideoPlacement>(new EnumEntitySerializer<VideoPlacement>());
                registry.Register<VideoLinearity>(new EnumEntitySerializer<VideoLinearity>());
                registry.Register<PlaybackMethod>(new EnumEntitySerializer<PlaybackMethod>());
                registry.Register<PlaybackCessation>(new EnumEntitySerializer<PlaybackCessation>());
                registry.Register<ProductionQuality>(new EnumEntitySerializer<ProductionQuality>());
                registry.Register<ContentDeliveryMethod>(new EnumEntitySerializer<ContentDeliveryMethod>());
                registry.Register<CompanionType>(new EnumEntitySerializer<CompanionType>());
                registry.Register<VolumeNormalizationMode>(new EnumEntitySerializer<VolumeNormalizationMode>());
                registry.Register<FeedType>(new EnumEntitySerializer<FeedType>());
                registry.Register<BlockedCreative>(new EnumEntitySerializer<BlockedCreative>());
                registry.Register<BlockedType>(new EnumEntitySerializer<BlockedType>());
                registry.Register<ExpandableDirection>(new EnumEntitySerializer<ExpandableDirection>());
                registry.Register<MediaRating>(new EnumEntitySerializer<MediaRating>());
                registry.Register<VideoProtocol>(new EnumEntitySerializer<VideoProtocol>());
                registry.Register<ApiFramework>(new EnumEntitySerializer<ApiFramework>());
                registry.Register<NoBidReason>(new EnumEntitySerializer<NoBidReason>());
                registry.Register<CreativeAttribute>(new EnumEntitySerializer<CreativeAttribute>());

                // Register the types
                registry.Register<Metric>(new MetricEntitySerializer());
                registry.Register<Segment>(new SegmentEntitySerializer());
                registry.Register<Regulation>(new RegulationEntitySerializer());
                registry.Register<Source>(new SourceEntitySerializer());
                registry.Register<Gender>(new GenderEntitySerializer());
                registry.Register<Geo>(new GeoEntitySerializer());
                registry.Register<Data>(new DataEntitySerializer());
                registry.Register<User>(new UserEntitySerializer());
                registry.Register<Device>(new DeviceEntitySerializer());
                registry.Register<Producer>(new ProducerEntitySerializer());
                registry.Register<Content>(new ContentEntitySerializer());
                registry.Register<Deal>(new DealEntitySerializer());
                registry.Register<PrivateMarketplace>(new PrivateMarketplaceEntitySerializer());
                registry.Register<Format>(new FormatEntitySerializer());
                registry.Register<Banner>(new BannerEntitySerializer());
                registry.Register<CompanionAd>(new CompanionAdEntitySerializer());
                registry.Register<Video>(new VideoEntitySerializer());
                registry.Register<Audio>(new AudioEntitySerializer());
                registry.Register<Impression>(new ImpressionEntitySerializer());
                registry.Register<Publisher>(new PublisherEntitySerializer());
                registry.Register<Site>(new SiteEntitySerializer());
                registry.Register<App>(new AppEntitySerializer());
                registry.Register<BidRequest>(new BidRequestEntitySerializer());
                registry.Register<Bid>(new BidEntitySerializer());
                registry.Register<SeatBid>(new SeatBidEntitySerializer());
                registry.Register<BidResponse>(new BidResponseEntitySerializer());
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