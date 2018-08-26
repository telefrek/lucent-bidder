using System;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Serializers;
using Lucent.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        public static void AddEntitySerializers(this IServiceProvider provider)
        {
            var registry = provider.GetRequiredService<ISerializationRegistry>();
            
            if(!registry.IsSerializerRegisterred<Campaign>())
                registry.Register<Campaign>(new CampaignSerializer());
        }
    }
}