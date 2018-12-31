using System;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.OpenRTB;
using Microsoft.AspNetCore.Http;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Custom exchange specific logic
    /// </summary>
    public abstract class AdExchange
    {
        /// <summary>
        /// Initializes the exchange with the current provider
        /// </summary>
        /// <param name="provider">The current provider</param>
        public abstract Task Initialize(IServiceProvider provider);

        /// <summary>
        /// Bids on the given request
        /// </summary>
        /// <param name="request">The bid to process</param>
        /// <param name="httpContext"></param>
        /// <returns>A fully formed response</returns>
        public abstract Task<BidResponse> Bid(BidRequest request, HttpContext httpContext);

        /// <summary>
        /// Gets the flag for suppressing the byte order marks during serialization
        /// </summary>
        public bool SuppressBOM { get; set; }

        /// <summary>
        /// Gets the identifier for the exchange
        /// </summary>
        public Guid ExchangeId { get; set; }

        /// <summary>
        /// Gets the exchange name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Format the OpenRTB Bid object
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public abstract Bid FormatBid(BidMatch bid, HttpContext httpContext);
    }
}