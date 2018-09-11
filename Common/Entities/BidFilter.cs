using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Entities
{
    public class BidFilter
    {
        public Filter[] GeoFilters { get; set; }
        public Filter[] ImpressionFilters { get; set; }
        public Filter[] UserFilters { get; set; }
        public Filter[] DeviceFilters { get; set; }
        public Filter[] SiteFilters { get; set; }
        public Filter[] AppFilters { get; set; }
    }
}