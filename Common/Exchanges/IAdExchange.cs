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
    public interface IAdExchange
    {
        /// <summary>
        /// Initializes the exchange with the current provider
        /// </summary>
        /// <param name="provider">The current provider</param>
        Task Initialize(IServiceProvider provider);

        /// <summary>
        /// Determines if the context is a match for the given exchange
        /// </summary>
        /// <param name="httpContext">The current request context</param>
        /// <returns>True if the request is from the exchange</returns>
        bool IsMatch(HttpContext httpContext);

        /// <summary>
        /// Bids on the given request
        /// </summary>
        /// <param name="request">The bid to process</param>
        /// <param name="httpContext"></param>
        /// <returns>A fully formed response</returns>
        Task<BidResponse> Bid(BidRequest request, HttpContext httpContext);

        /// <summary>
        /// Gets the flag for suppressing the byte order marks during serialization
        /// </summary>
        bool SuppressBOM { get; }

        /// <summary>
        /// Gets the identifier for the exchange
        /// </summary>
        Guid ExchangeId { get; set; }

        /// <summary>
        /// Gets the exchange name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the load order
        /// </summary>
        /// <value></value>
        int Order { get; }

        /// <summary>
        /// Format the OpenRTB Bid object
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Bid FormatBid(BidMatch bid, HttpContext httpContext);
    }
}