using System.Linq;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class BidRequest
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
        public Impression[] Impressions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Site Site { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public App App { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Device Device { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public User User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool TestFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public AuctionType AuctionType { get; set; } = AuctionType.SecondPrice;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Milliseconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] WhitelistBuyers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] BlockedBuyers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool AllImpressions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] Currencies { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] Languages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] BlockedCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] BlockedAdvertisers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] BlockedApplications { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Source Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Regulation Regulations { get; set; }
    }
}