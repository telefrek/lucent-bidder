using System.Collections.Generic;

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
    }
}