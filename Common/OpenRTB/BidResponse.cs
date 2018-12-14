using Lucent.Common.Serialization;

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
        [SerializationProperty(1, "id")]
        public string Id { get; set; }
        /// <summary>
        /// Bids associates with this response
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "seatbid")]
        public SeatBid[] Bids { get; set; }
        /// <summary>
        /// The unique response id
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "bidid")]
        public string CorrelationId { get; set; }
        /// <summary>
        /// The bid currency
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "cur")]
        public string Currency { get; set; } = "USD";
        /// <summary>
        /// Any customer user data in base85 encoded format
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "customdata")]
        public string CustomData85 { get; set; }
        /// <summary>
        /// No bid reason if there are no bids in the response
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "nbr")]
        public NoBidReason NoBidReason { get; set; }
    }
}