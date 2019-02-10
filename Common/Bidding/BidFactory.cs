using System;
using Lucent.Common.Entities;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Implementation of the IBidFactory interface
    /// </summary>
    public class BidFactory : IBidFactory
    {
        IServiceProvider _provider;

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="provider">The current provider</param>
        public BidFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        public ICampaignBidder CreateBidder(Campaign campaign)
        {
            return _provider.CreateInstance<CampaignBidder>(campaign);
        }
    }
}