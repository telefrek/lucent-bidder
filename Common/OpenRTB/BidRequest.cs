using System.Linq;

namespace Lucent.Core.Entities.OpenRTB
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
        public BidResponse Response { get; set; }
        public long Timestamp { get; set; }
        public Geo Geography
        {
            get
            {
                Geo uGeo = User != null ? User.Geo : null;
                Geo dGeo = Device != null ? Device.Geography : null;

                return dGeo ?? uGeo;
            }
        }

        public string[] Categories
        {
            get
            {
                if (App != null)
                {
                    return App.AppCategories ?? new string[0]
                        .Concat(App.PageCategories ?? new string[0])
                        .Concat(App.SectionCategories ?? new string[0]).ToArray();
                }
                else if (Site != null)
                {
                    return Site.SiteCategories ?? new string[0]
                        .Concat(Site.SectionCategories ?? new string[0])
                        .Concat(Site.PageCategories ?? new string[0]).ToArray();
                }

                return new string[0];
            }
        }
    }
}