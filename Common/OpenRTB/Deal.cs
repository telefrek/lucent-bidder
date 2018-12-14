using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Deal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "bidfloor")]
        public double BidFloor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "bidfloorcur")]
        public string BidFloorCur { get; set; } = "USD";

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "wseat")]
        public string[] WhitelistBuyers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "wadomain")]
        public string[] WhitelistDomains { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "at")]
        public AuctionType AuctionType { get; set; }
    }
}