using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBiddingManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        List<ICampaignBidder> Bidders { get; }

        /// <summary>
        /// Bind this manager to a specific exchange
        /// </summary>
        /// <param name="exchangeId">The exchange to set to</param>
        /// <returns>Task for async codde</returns>
        Task BindTo(Guid exchangeId);

        /// <summary>
        /// Check to see if the exchange is available for bidding
        /// </summary>
        /// <returns></returns>
        Task<bool> CanBid();
    }
}