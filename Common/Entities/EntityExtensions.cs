using Lucent.Common.Entities;
using Lucent.Common.Entities.Serializers;
using Lucent.Common.Filters;
using Lucent.Common.Filters.Serializers;
using Lucent.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        public static IServiceCollection AddEntitySerializers(this IServiceCollection provider)
        {
            var registry = provider.BuildServiceProvider().GetRequiredService<ISerializationRegistry>();
            if (!registry.IsSerializerRegisterred<Campaign>())
            {
                registry.Register<Campaign>(new CampaignSerializer());
                registry.Register<CampaignSchedule>(new CampaignScheduleSerializer());
                registry.Register<Creative>(new CreativeSearializer());
                registry.Register<CreativeContent>(new CreativeContentSerializer());
                registry.Register<Filter>(new FilterSerializer());
                registry.Register<BidFilter>(new BidFilterSerializer());
            }
            return provider;
        }
    }
}