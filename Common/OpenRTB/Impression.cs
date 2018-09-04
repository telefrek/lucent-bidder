namespace Lucent.Common.OpenRTB
{
    public class Impression
    {
        public string ImpressionId { get; set; }
        public Metric[] Metrics { get; set; }
        public Banner Banner { get; set; }
        public Video Video { get; set; }
        public Audio Audio { get; set; }
        public PrivateMarketplace PrivateMarketplace { get; set; }
        public string DisplayManager { get; set; }
        public string DisplayManagerVersion { get; set; }
        public bool FullScreen { get; set; }
        public string TagId { get; set; }
        public double BidFloor { get; set; }
        public string BidCurrency { get; set; } = "USD";
        public bool IsClickNative { get; set; }
        public bool IsHttpsRequired { get; set; }
        public string[] IFrameBusters { get; set; }
        public int ExpectedAuctionDelay { get; set; }
        public dynamic Ext { get; set; }
    }
}