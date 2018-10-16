using System;
using System.Threading.Tasks;
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
        void Initialize(IServiceProvider provider);

        /// <summary>
        /// Determines if the context is a match for the given exchange
        /// </summary>
        /// <param name="context">The current request context</param>
        /// <returns>True if the request is from the exchange</returns>
        bool IsMatch(HttpContext context);

        /// <summary>
        /// Bids on the given request
        /// </summary>
        /// <param name="request">The bid to process</param>
        /// <returns>A fully formed response</returns>
        Task<BidResponse> Bid(BidRequest request);

        /// <summary>
        /// Gets the flag for suppressing the byte order marks during serialization
        /// </summary>
        bool SuppressBOM { get; }

        /// <summary>
        /// Gets the identifier for the exchange
        /// </summary>
        Guid ExchangeId { get; }

        /// <summary>
        /// Gets the exchange name
        /// </summary>
        string Name { get; }
    }
}