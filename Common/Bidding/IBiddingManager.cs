using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Exchanges;

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
        /// Initialize the manager for the exchange
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        Task Initialize(AdExchange exchange);

        /// <summary>
        /// Check to see if the exchange is available for bidding
        /// </summary>
        /// <returns></returns>
        Task<bool> CanBid();
    }
}