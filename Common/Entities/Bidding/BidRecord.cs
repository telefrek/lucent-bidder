using System;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;

namespace Lucent.Common.Entities.Bidding
{
    /// <summary>
    /// Class representing a bid
    /// </summary>
    public class BidRecord
    {
        /// <summary>
        /// The bid
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "bid")]
        public Bid Bid { get; set; }

        /// <summary>
        /// The exchange
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "eid")]
        public string ExchangeId { get; set; }

        /// <summary>
        /// The original request
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "req")]
        public BidRequest Request { get; set; }

        /// <summary>
        /// The request timestamp
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "ts")]
        public DateTime Timestamp {get;set;} = DateTime.UtcNow;
    }
}