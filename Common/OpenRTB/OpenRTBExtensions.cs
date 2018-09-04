using Lucent.Common.OpenRTB;
using Lucent.Common.OpenRTB.Serializers;
using Lucent.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenRTBSerializers(this IServiceCollection provider)
        {
            var registry = provider.BuildServiceProvider().GetRequiredService<ISerializationRegistry>();

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

            return provider;
        }
    }
}