using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;
using Lucent.Common.Scoring;
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
        IScoringService _scoringService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="c"></param>
        /// <param name="logger"></param>
        /// <param name="ledgerManager"></param>
        /// <param name="scoringService"></param>
        public CampaignBidder(Campaign c, ILogger<CampaignBidder> logger, IBudgetLedgerManager ledgerManager, IScoringService scoringService)
        {
            _campaign = c;
            _log = logger;
            _ledger = ledgerManager.GetLedger(c);
            _scoringService = scoringService;
        }

        /// <summary>
        /// Gets the associated campaign
        /// </summary>
        public Campaign Campaign => _campaign;

        static BidContext[] NO_MATCHES = new BidContext[0];

        /// <summary>
        /// Filters the bid request to get the set of Impressions that can be bid on
        /// </summary>
        /// <param name="request">The request to filder</param>
        /// <param name="httpContext"></param>
        /// <returns>The set of impressions that weren't filtered</returns>
        public async Task<BidContext[]> BidAsync(BidRequest request, HttpContext httpContext)
        {
            // Apply campaign filters
            if (_campaign.IsFiltered(request))
                return NO_MATCHES;

            var impList = new List<BidContext>();
            var allMatched = true;

            // Make sure there is at least one content per impression
            foreach (var imp in request.Impressions)
            {
                // Get the potential matches
                var matches = _campaign.Creatives.SelectMany(c => c.Contents.Where(cc => !cc.Filter(imp))
                    .Select(cc => 
                    {
                        var ctx = BidContext.Create(httpContext);
                        ctx.Request = request;
                        ctx.Impression = imp;
                        ctx.Campaign = _campaign;
                        ctx.Creative = c;
                        ctx.Content = cc;
                        ctx.BidId = SequentialGuid.NextGuid();
                        
                        return ctx;
                    })).ToList();

                allMatched &= matches.Count > 0;
                impList.AddRange(matches);
            }

            // Ensure if sold as a bundle, we have all impressions, otherwise return matched or none
            if (request.AllImpressions && !allMatched)
                return NO_MATCHES;

            // Get a score for the campaign to the request
            var score = await _scoringService.Score(Campaign, request);

            // Need some uri building
            var baseUri = new UriBuilder
            {
                Scheme = httpContext.Request.Scheme,
                Host = httpContext.Request.Host.Value,
            };

            return impList.Select(bidContext =>
            {
                // TODO: This is a terrible cpm calculation lol
                var cpm = score * Campaign.ConversionPrice;
                if (cpm >= bidContext.Impression.BidFloor)
                {
                    bidContext.BaseUri = baseUri;
                    bidContext.Bid = new Bid
                    {
                        ImpressionId = bidContext.Impression.ImpressionId,
                        Id = bidContext.BidId.ToString(),
                        CPM = cpm,
                        WinUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Win) + "&cpm=${AUCTION_PRICE}").AbsoluteUri,
                        LossUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Loss)).AbsoluteUri,
                        BillingUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Impression)).AbsoluteUri,
                        H = bidContext.Content.H,
                        W = bidContext.Content.W,
                        AdDomain = bidContext.Campaign.AdDomains.ToArray(),
                        BidExpiresSeconds = 300,
                        Bundle = bidContext.Campaign.BundleId,
                        ContentCategories = bidContext.Content.Categories,
                        ImageUrl = bidContext.Content.RawUri,
                        AdId = bidContext.Creative.Id,
                        CreativeId = bidContext.Creative.Id,
                        CampaignId = bidContext.Campaign.Id,
                    };

                    return bidContext;
                }
                return null;
            }).Where(b => b != null).ToArray();
        }
    }
}