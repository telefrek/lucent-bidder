namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// OpenRTB BidResponse
    /// </summary>
    public class BidResponse
    {
        /// <summary>
        /// The original request id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Bids associates with this response
        /// </summary>
        /// <value></value>
        public SeatBid[] Bids { get; set; }
        /// <summary>
        /// The unique response id
        /// </summary>
        /// <value></value>
        public string CorrelationId { get; set; }
        /// <summary>
        /// The bid currency
        /// </summary>
        /// <value></value>
        public string Currency { get; set; } = "USD";
        /// <summary>
        /// Any customer user data in base85 encoded format
        /// </summary>
        /// <value></value>
        public string CustomData85 { get; set; }
        /// <summary>
        /// No bid reason if there are no bids in the response
        /// </summary>
        /// <value></value>
        public NoBidReason NoBidReason { get; set; }
    }
}