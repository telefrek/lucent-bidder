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
        Histogram _bidderLatency = Metrics.CreateHistogram("campaign_bidder_latency", "Campaign bidder latency", new HistogramConfiguration
        {
            Buckets = MetricBuckets.LOW_LATENCY_10_MS
        });

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

            // Ensure filter is hydrated
            if (_campaign.BidFilter != null)
                _campaign.IsFiltered = (_campaign.JsonFilters ?? new JsonFilter[0]).MergeFilter(_campaign.BidFilter).GenerateFilter();
            if (_campaign.BidTargets != null)
                _campaign.IsTargetted = (_campaign.JsonTargets ?? new JsonFilter[0]).MergeTarget(_campaign.BidTargets).GenerateTargets();

            _log = logger;
            _scoringService = scoringService;
            _budgetManager = budgetManager;
            _campaignId = c.Id;
            _budgetCache = budgetCache;
            _campaignBudget = LocalBudget.Get(_campaignId);

            _budgetManager.RegisterHandler(new BudgetEventHandler
            {
                IsMatch = (e) => { return e.EntityId == _campaignId; },
                HandleAsync = async (e) =>
                {
                    var status = await _budgetCache.TryGetRemaining(_campaignId);
                    if (status.Successful)
                    {
                        LocalBudget.Get(_campaignId).Budget.Sync(status.Remaining);
                        _isBudgetExhausted = status.Remaining <= 0;
                    }
                    else
                    {
                        _isBudgetExhausted = true;
                        _log.LogWarning("Failed to sync campaign budget for {0}, stopping to be safe", _campaign.Name);
                    }

                    _log.LogInformation("Budget change for campaign : {0} ({1})", _campaign.Name, _isBudgetExhausted);
                }
            });
        }

        /// <summary>
        /// Gets the associated campaign
        /// </summary>
        public Campaign Campaign => _campaign;

        static BidContext[] NO_MATCHES = new BidContext[0];
        static Random _rng = new Random();

        /// <summary>
        /// Filters the bid request to get the set of Impressions that can be bid on
        /// </summary>
        /// <param name="request">The request to filder</param>
        /// <param name="httpContext"></param>
        /// <returns>The set of impressions that weren't filtered</returns>
        public async Task<BidContext[]> BidAsync(BidRequest request, HttpContext httpContext)
        {
            using (var histogram = _bidderLatency.CreateContext())
            {
                if (_campaign.Status != CampaignStatus.Active)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_inactive").Inc();
                    return NO_MATCHES;
                }

                if (_isBudgetExhausted = _campaignBudget.Budget.GetDouble() <= 0)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_suspended").Inc();
                    await _budgetManager.RequestAdditional(_campaign.Id, EntityType.Campaign);
                    return NO_MATCHES;
                }

                if (_campaign.Schedule == null)
                {
                    BidCounters.NoBidReason.WithLabels("no_campaign_schedule").Inc();
                    return NO_MATCHES;
                }

                if (DateTime.UtcNow < _campaign.Schedule.StartDate)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_not_started").Inc();
                    return NO_MATCHES;
                }

                if (!_campaign.Schedule.EndDate.IsNullOrDefault() && DateTime.UtcNow > _campaign.Schedule.EndDate)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_ended").Inc();
                    return NO_MATCHES;
                }

                // Apply campaign filters and check budget
                if (_campaign.IsFiltered(request))
                {
                    BidCounters.NoBidReason.WithLabels("campaign_filtered").Inc();
                    return NO_MATCHES;
                }

                if (!_campaign.IsTargetted(request))
                {
                    BidCounters.NoBidReason.WithLabels("campaign_target_failed").Inc();
                    return NO_MATCHES;
                }

                var impList = new List<BidContext>();
                var allMatched = true;

                // Make sure there is at least one content per impression
                foreach (var imp in request.Impressions.Where(i => i.BidFloor <= _campaign.MaxCPM))
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

                // Need some uri building
                var baseUri = new UriBuilder
                {
                    Scheme = "https",
                    Host = httpContext.Request.Host.Value,
                };

                // Get the current stats
                var stats = CampaignStats.Get(_campaignId);

                var ret = impList.Select(bidContext =>
                {
                    var cpm = Math.Round(Campaign.Actions.First().Payout * stats.CTR * 1000 * .85, 4);
                    if (cpm == 0 || cpm > Campaign.MaxCPM) cpm = Campaign.MaxCPM;

                    if (Campaign.TargetCPM < cpm && Campaign.TargetCPM >= bidContext.Impression.BidFloor)
                    {
                        cpm = (cpm - Campaign.TargetCPM) * _rng.NextDouble() + Campaign.TargetCPM;
                        cpm = Math.Max(cpm, bidContext.Impression.BidFloor);
                    }

                    if (cpm >= bidContext.Impression.BidFloor)
                    {
                        bidContext.BaseUri = baseUri;
                        bidContext.BidDate = DateTime.UtcNow;
                        bidContext.Bid = new Bid
                        {
                            ImpressionId = bidContext.Impression.ImpressionId,
                            Id = bidContext.BidId.ToString(),
                            CPM = cpm,
                            WinUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Win)).AbsoluteUri + "&cpm=${AUCTION_PRICE}",
                            LossUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Loss)).AbsoluteUri,
                            BillingUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Impression)).AbsoluteUri,
                            H = bidContext.Content.H,
                            W = bidContext.Content.W,
                            AdDomain = bidContext.Campaign.AdDomains.ToArray(),
                            BidExpiresSeconds = 300,
                            Bundle = bidContext.Campaign.BundleId,
                            ContentCategories = bidContext.Content.Categories,
                            ImageUrl = bidContext.Content.CreativeUri,
                            AdId = bidContext.Creative.Id,
                            CreativeId = bidContext.Creative.Id + "." + bidContext.Content.Id,
                            CampaignId = bidContext.Campaign.Id,
                        };
                        bidContext.CPM = cpm;

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
}