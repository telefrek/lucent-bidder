using System.Threading.Tasks;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Public bidder interface
    /// </summary>
    public interface ICampaignBidder
    {
        /// <summary>
        /// Create a bid for the given impression
        /// </summary>
        /// <param name="impression"></param>
        /// <returns>A bid on the given impression</returns>
        Task<Bid> BidAsync(Impression impression);

        /// <summary>
        /// Check to see if the bidder wants to bid on part/all of this request
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The set of impressions this bidder wants</returns>
        Impression[] FilterImpressions(BidRequest request);
    }
}