namespace Lucent.Common.OpenRTB
{
    public class SeatBid
    {
        public Bid[] Bids { get; set; }
        public string BuyerId { get; set; } = "lucentbidder";
        public bool IsGrouped { get; set; }
    }
}