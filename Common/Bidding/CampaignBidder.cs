using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Microsoft.AspNetCore.Http;
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
        ICampaignLedger _ledger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="c"></param>
        /// <param name="logger"></param>
        /// <param name="ledgerManager"></param>
        public CampaignBidder(Campaign c, ILogger<CampaignBidder> logger, IBudgetLedgerManager ledgerManager)
        {
            _campaign = c;
            _log = logger;
            _ledger = ledgerManager.GetLedger(c);
        }

        /// <summary>
        /// Gets the associated campaign
        /// </summary>
        public Campaign Campaign => _campaign;

        static BidMatch[] NO_MATCHES = new BidMatch[0];

        /// <summary>
        /// Filters the bid request to get the set of Impressions that can be bid on
        /// </summary>
        /// <param name="request">The request to filder</param>
        /// <param name="httpContext"></param>
        /// <returns>The set of impressions that weren't filtered</returns>
        public async Task<BidMatch[]> BidAsync(BidRequest request, HttpContext httpContext)
        {
            // Apply campaign filters
            if (_campaign.IsFiltered(request))
                return NO_MATCHES;

            var impList = new List<BidMatch>();
            var allMatched = true;

            // Make sure there is at least one content per impression
            foreach (var imp in request.Impressions)
            {
                // Get the potential matches
                var matches = _campaign.Creatives.SelectMany(c => c.Contents.Where(cc => !cc.Filter(imp)).Select(cc => new BidMatch { Impression = imp, Campaign = _campaign, Creative = c, Content = cc })).ToList();

                allMatched &= matches.Count > 0;
                impList.AddRange(matches);
            }

            // Ensure if sold as a bundle, we have all impressions, otherwise return matched or none
            if (request.AllImpressions && !allMatched)
                return NO_MATCHES;

            // Scoring to make async stop complaining
            await Task.Delay(10);

            return impList.Select(bm =>
            {
                bm.RawBid = new Bid
                {
                    ImpressionId = bm.Impression.ImpressionId,
                    Id = SequentialGuid.NextGuid().ToString(),
                    CPM = bm.Impression.BidFloor,
                    WinUrl = httpContext.Request.Host.Value + "/v1/win",
                    LossUrl = httpContext.Request.Host.Value + "/v1/loss",
                    BillingUrl = httpContext.Request.Host.Value + "/v1/bill",
                    H = bm.Content.H,
                    W = bm.Content.W,
                    AdDomain = bm.Campaign.AdDomains,
                    BidExpiresSeconds = 300,
                    Bundle = bm.Campaign.BundleId,
                    ContentCategories = bm.Content.Categories,
                    ImageUrl = bm.Content.RawUri,
                    AdId = bm.Creative.Id,
                    CreativeId = bm.Creative.Id,
                    CampaignId = bm.Campaign.Id,
                };
                return bm;
            }).ToArray();
        }
    }
}