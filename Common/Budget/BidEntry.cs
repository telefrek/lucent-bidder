using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Wraps a bid with request information
    /// </summary>
    public class BidEntry
    {
        /// <summary>
        /// The bid
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "bidContext")]
        public string BidContext { get; set; }

        /// <summary>
        /// The request
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "requestId")]
        public string RequestId { get; set; }

        /// <summary>
        /// The bid cost
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "cost")]
        public double Cost { get; set; }
    }
}