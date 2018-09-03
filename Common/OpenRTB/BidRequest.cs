using System.Linq;

namespace Lucent.Common.OpenRTB
{
    public class BidRequest
    {
        public string Id { get; set; }
        public Impression[] Impressions { get; set; }
        public Site Site { get; set; }
        public App App { get; set; }
        public Device Device { get; set; }
        public User User { get; set; }
        public bool TestFlag { get; set; }
        public AuctionType AuctionType { get; set; } = AuctionType.SecondPrice;
        public int Milliseconds { get; set; }
        public string[] WhitelistBuyers { get; set; }
        public string[] BlockedBuyers { get; set; }
        public bool AllImpressions { get; set; }
        public string[] Currencies { get; set; }
        public string[] Languages { get; set; }
        public string[] BlockedCategories { get; set; }
        public string[] BlockedAdvertisers { get; set; }
        public string[] BlockedApplications { get; set; }
        public Source Source { get; set; }
        public Regulation Regulations { get; set; }
    }
}