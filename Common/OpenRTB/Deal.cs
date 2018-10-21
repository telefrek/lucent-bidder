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
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double BidFloor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BidFloorCur { get; set; } = "USD";

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public AuctionType AuctionType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] WhitelistBuyers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] WhitelistDomains { get; set; }
    }
}