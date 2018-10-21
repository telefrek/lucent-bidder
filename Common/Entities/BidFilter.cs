using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class BidFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Filter[] GeoFilters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Filter[] ImpressionFilters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Filter[] UserFilters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Filter[] DeviceFilters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Filter[] SiteFilters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Filter[] AppFilters { get; set; }
    }
}