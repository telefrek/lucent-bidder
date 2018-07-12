namespace Lucent.Common.OpenRTB
{
    public class BidResponse
    {
        public string Id { get; set; }
        public SeatBid[] Bids { get; set; }
        public string CorrelationId { get; set; }
        public string Currency { get; set; } = "USD";
        public string CustomData85 { get; set; }
        public NoBidReason NoBidReason { get; set; }
    }
}