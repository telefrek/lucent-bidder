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
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ImpressionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double CPM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string WinUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BillingUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string LossUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string AdMarkup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string AdId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] AdDomain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Bundle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string CampaignId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string CreativeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string TacticId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] ContentCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public CreativeAttribute[] CreativeAttributes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ApiFramework API { get; set; } = ApiFramework.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public VideoProtocol Protocol { get; set; } = VideoProtocol.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public MediaRating MediaRating { get; set; } = MediaRating.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DealId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsFlexAd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int WRatio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int HRatio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int BidExpiresSeconds { get; set; } = 300; // default is 5 minutes
    }
}