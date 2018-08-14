using System.Threading.Tasks;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Exchange specific bidding
    /// </summary>
    public interface IExchangeBidder
    {
        /// <summary>
        /// Gets the exchange identifier
        /// </summary>
        string ExchangeId { get; }

        /// <summary>
        /// Creates a correctly formatted response for the given BidRequest
        /// </summary>
        /// <param name="request">The BidRequest to process</param>
        /// <returns>An OpenRTB BidResponse</returns>
        Task<BidResponse> BidAsync(BidRequest request);
    }
}