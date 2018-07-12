namespace Lucent.Core.Entities.OpenRTB
{
    public class Bid
    {
        public string Id { get; set; }
        public string ImpressionId { get; set; }
        public double CPM { get; set; }
        public string WinUrl { get; set; }
        public string BillingUrl { get; set; }
        public string LossUrl { get; set; }
        public string AdMarkup { get; set; }
        public string AdId { get; set; }
        public string[] AdDomain { get; set; }
        public string Bundle { get; set; }
        public string ImageUrl { get; set; }
        public string CampaignId { get; set; }
        public string CreativeId { get; set; }
        public string TacticId { get; set; }
        public string[] ContentCategories { get; set; }
        public CreativeAttribute[] CreativeAttributes { get; set; }
        public ApiFramework API { get; set; } = ApiFramework.Invalid;
        public VideoProtocol Protocol { get; set; } = VideoProtocol.Invalid;
        public MediaRating MediaRating { get; set; } = MediaRating.Invalid;
        public string Language { get; set; }
        public string DealId { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public bool IsFlexAd { get; set; }
        public int WRatio { get; set; }
        public int HRatio { get; set; }
        public int BidExpiresSeconds { get; set; } = 300; // default is 5 minutes
    }
}