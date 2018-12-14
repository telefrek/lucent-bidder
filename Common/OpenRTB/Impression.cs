using Lucent.Common.Serialization;

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
        [SerializationProperty(1, "id")]
        public string ImpressionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "banner")]
        public Banner Banner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "video")]
        public Video Video { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "displaymanager")]
        public string DisplayManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "displaymanagerver")]
        public string DisplayManagerVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "instl")]
        public bool FullScreen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "tagid")]
        public string TagId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "bidfloor")]
        public double BidFloor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(9, "bidfloorcur")]
        public string BidCurrency { get; set; } = "USD";

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "iframebuster")]
        public string[] IFrameBusters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "pmp")]
        public PrivateMarketplace PrivateMarketplace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// /// <value></value>
        [SerializationProperty(12, "secure")]
        public bool IsHttpsRequired { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(14, "exp")]
        public int ExpectedAuctionDelay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(15, "audio")]
        public Audio Audio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(16, "clickbrowser")]
        public bool IsClickNative { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(17, "metric")]
        public Metric[] Metrics { get; set; }
    }
}