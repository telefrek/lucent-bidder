using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Budget;

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
        /// Check to see if the exchange is available for bidding
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> CanBid(string id);
    }
}