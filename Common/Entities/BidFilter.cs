using Lucent.Common.OpenRTB;

namespace Lucent.Common.Entities
{
    public class BidFilter
    {
        public Filter<Geo>[] GeoFilters { get; set; }
        public Filter<Impression>[] ImpressionFilters { get; set; }
        public Filter<User>[] UserFilters { get; set; }
        public Filter<Device>[] DeviceFilters { get; set; }
        public Filter<Site>[] SiteFilters { get; set; }
        public Filter<App>[] AppFilters { get; set; }
    }
}