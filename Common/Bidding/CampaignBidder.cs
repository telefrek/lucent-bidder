using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Events;
using Lucent.Common.Exchanges;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;
using Lucent.Common.Scoring;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Default campaign bidder implementation
    /// </summary>
    public class CampaignBidder : ICampaignBidder
    {
        Campaign _campaign;
        string _campaignId;
        bool _isBudgetExhausted;

        ILogger<CampaignBidder> _log;
        IScoringService _scoringService;
        IBudgetManager _budgetManager;
        IBudgetCache _budgetCache;
        LocalBudget _campaignBudget;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="c"></param>
        /// <param name="logger"></param>
        /// <param name="scoringService"></param>
        /// <param name="budgetManager"></param>
        /// <param name="watcher"></param>
        /// <param name="storageManager"></param>
        /// <param name="budgetCache"></param>
        public CampaignBidder(Campaign c, ILogger<CampaignBidder> logger, IScoringService scoringService, IBudgetManager budgetManager, IEntityWatcher watcher, IStorageManager storageManager, IBudgetCache budgetCache)
        {
            _campaign = c;
            _log = logger;
            _scoringService = scoringService;
            _budgetManager = budgetManager;
            _campaignId = c.Id;
            _budgetCache = budgetCache;
            _campaignBudget = LocalBudget.Get(_campaignId);

            _budgetManager.OnStatusChanged = (e) =>
            {
                if (e.EntityId == _campaignId)
                {
                    var rem = _budgetCache.TryGetBudget(_campaignId).Result;
                    LocalBudget.Get(_campaignId).Last = rem;

                    _isBudgetExhausted = rem <= 0d;
                    _log.LogInformation("Budget change {0} ({1})", e.EntityId, _isBudgetExhausted);
                }
                return Task.CompletedTask;
            };
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
            // Apply campaign filters and check budget
            if (_campaign.IsFiltered(request))
            {
                BidCounters.NoBidReason.WithLabels("campaign_filtered").Inc();
                return NO_MATCHES;
            }

            if (_isBudgetExhausted |= _campaignBudget.IsExhausted())
            {
                BidCounters.NoBidReason.WithLabels("no_campaign_budget").Inc();
                await _budgetManager.RequestAdditional(_campaign.Id);
                return NO_MATCHES;
            }

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
                        ctx.ExchangeId = ctx.Exchange.ExchangeId;
                        ctx.RequestId = request.Id;
                        ctx.CampaignId = Guid.Parse(Campaign.Id);

                        return ctx;
                    })).ToList();

                allMatched &= matches.Count > 0;
                impList.AddRange(matches);
            }

            // Ensure if sold as a bundle, we have all impressions, otherwise return matched or none
            if (request.AllImpressions && !allMatched)
            {
                BidCounters.NoBidReason.WithLabels("not_all_matched").Inc();
                return NO_MATCHES;
            }

            // Get a score for the campaign to the request
            var score = await _scoringService.Score(Campaign, request);

            // Need some uri building
            var baseUri = new UriBuilder
            {
                Scheme = httpContext.Request.Scheme,
                Host = httpContext.Request.Host.Value,
            };

            var ret = impList.Select(bidContext =>
            {
                // TODO: This is a terrible cpm calculation lol
                var cpm = Math.Round(Math.Max(0.5, score * Campaign.ConversionPrice), 4);
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

            if (ret.Length == 0)
                BidCounters.NoBidReason.WithLabels("no_creative_match").Inc();

            return ret;
        }
    }
}