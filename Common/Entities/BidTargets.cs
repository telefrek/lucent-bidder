using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Filters;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class BidTargets
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "app")]
        public Target[] AppTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "device")]
        public Target[] DeviceTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "geo")]
        public Target[] GeoTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "impression")]
        public Target[] ImpressionTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "site")]
        public Target[] SiteTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "user")]
        public Target[] UserTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "banner")]
        public Target[] BannerTargets { get; set; }
    }
}