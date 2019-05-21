using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Microsoft.AspNetCore.Http;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Public bidder interface
    /// </summary>
    public interface ICampaignBidder
    {
        /// <summary>
        /// Gets the associated campaign
        /// </summary>
        /// <value></value>
        Campaign Campaign { get; }

        /// <summary>
        /// Check to see if the bidder wants to bid on part/all of this request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpContext"></param>
        /// <returns>The set of impressions this bidder wants</returns>
        Task<BidContext[]> BidAsync(BidRequest request, HttpContext httpContext);
    }
}