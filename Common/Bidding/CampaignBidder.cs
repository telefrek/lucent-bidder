using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Default campaign bidder implementation
    /// </summary>
    public class CampaignBidder : ICampaignBidder
    {
        Campaign _campaign;
        ILogger<CampaignBidder> _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="c"></param>
        /// <param name="logger"></param>
        public CampaignBidder(Campaign c, ILogger<CampaignBidder> logger)
        {
            _campaign = c;
            _log = logger;
        }

        /// <summary>
        /// Gets the associated campaign
        /// </summary>
        public Campaign Campaign => _campaign;

        /// <summary>
        /// Bid on the given impression
        /// </summary>
        /// <param name="impression">The pre-screened impression to bid on</param>
        /// <returns>A bid for the impression</returns>
        public Task<Bid> BidAsync(Impression impression)
        {
            // Need to generate the bid at some point...
            return Task.FromResult(new Bid
            {
                CPM = 1.0,
                BidExpiresSeconds = 5,
            });
        }

        static readonly Impression[] EMPTY_IMPRESSION = new Impression[0];

        /// <summary>
        /// Filters the bid request to get the set of Impressions that can be bid on
        /// </summary>
        /// <param name="request">The request to filder</param>
        /// <returns>The set of impressions that weren't filtered</returns>
        public Impression[] FilterImpressions(BidRequest request)
        {
            if (_campaign.IsFiltered(request))
                return EMPTY_IMPRESSION;

            var impList = new List<Impression>();

            // Make sure there is at least one content per impression
            foreach (var imp in request.Impressions)
                if (_campaign.Creatives.Any(c => c.Contents.Any(cc => !cc.Filter(imp))))
                    impList.Add(imp);

            // Ensure if sold as a bundle, we have all impressions, otherwise return matched or none
            return request.AllImpressions ? impList.Count == request.Impressions.Length ? impList.ToArray() : EMPTY_IMPRESSION : impList.ToArray();
        }
    }
}