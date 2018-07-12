namespace Lucent.Common.OpenRTB
{
    public class Deal
    {
        public string Id { get; set; }
        public double BidFloor { get; set; }
        public string BidFloorCur { get; set; } = "USD";
        public AuctionType AuctionType { get; set; }
        public string[] WhitelistBuyers { get; set; }
        public string[] WhitelistDomains { get; set; }
    }
}