using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Bid
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
        [SerializationProperty(2, "impid")]
        public string ImpressionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "price")]
        public double CPM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "adid")]
        public string AdId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "nurl")]
        public string WinUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "adm")]
        public string AdMarkup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "adomain")]
        public string[] AdDomain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "iurl")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(9, "cid")]
        public string CampaignId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "crid")]
        public string CreativeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "attr")]
        public CreativeAttribute[] CreativeAttributes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(13, "dealid")]
        public string DealId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(14, "bundle")]
        public string Bundle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(15, "cat")]
        public string[] ContentCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(16, "w")]
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(17, "h")]
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(18, "api")]
        public ApiFramework API { get; set; } = ApiFramework.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(19, "protocol")]
        public VideoProtocol Protocol { get; set; } = VideoProtocol.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(20, "qagmediarating")]
        public MediaRating MediaRating { get; set; } = MediaRating.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(21, "exp")]
        public int BidExpiresSeconds { get; set; } = 300; // default is 5 minutes


        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(22, "burl")]
        public string BillingUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(23, "lurl")]
        public string LossUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(24, "tactic")]
        public string TacticId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(25, "language")]
        public string Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(26, "wratio")]
        public int WRatio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(27, "hratio")]
        public int HRatio { get; set; }
    }
}