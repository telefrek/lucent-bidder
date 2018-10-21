namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Impression
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ImpressionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Metric[] Metrics { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Banner Banner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Video Video { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Audio Audio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public PrivateMarketplace PrivateMarketplace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DisplayManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DisplayManagerVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool FullScreen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string TagId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double BidFloor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BidCurrency { get; set; } = "USD";

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsClickNative { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// /// <value></value>
        public bool IsHttpsRequired { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] IFrameBusters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int ExpectedAuctionDelay { get; set; }
    }
}