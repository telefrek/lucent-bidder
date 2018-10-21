namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class SeatBid
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Bid[] Bids { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BuyerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsGrouped { get; set; }
    }
}