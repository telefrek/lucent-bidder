using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;
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
        /// <param name="budgetManager"></param>
        /// <param name="watcher"></param>
        /// <param name="storageManager"></param>
        /// <param name="budgetCache"></param>
        public CampaignBidder(Campaign c, ILogger<CampaignBidder> logger, IBudgetManager budgetManager, IEntityWatcher watcher, IStorageManager storageManager, IBudgetCache budgetCache)
        {
            _campaign = c;
            _log = logger;

            // Ensure filter is hydrated
            try
            {
                _campaign.IsFiltered = (_campaign.JsonFilters ?? new JsonFilter[0]).MergeFilter(_campaign.BidFilter).GenerateFilter();
            }
            catch (Exception e)
            {
                _campaign.IsFiltered = (r) => true;
                _log.LogError(e, "failed to setup filters for {0}", _campaign.Name);
            }

            try
            {
                _log.LogInformation("Generating expression for : " + _campaign.Name);
                _campaign.GetModifier = (_campaign.JsonTargets ?? new JsonFilter[0]).MergeTarget(_campaign.BidTargets, _log).GenerateTargets(_campaign.TargetCPM == 0 ? _campaign.MaxCPM / 2 : _campaign.TargetCPM, _log);
            }
            catch (Exception e)
            {
                _log.LogError(e, "failed to setup targets for {0}", _campaign.Name);
            }

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
                    BidCounters.NoBidReason.WithLabels("campaign_inactive", Campaign.Name).Inc();
                    return NO_MATCHES;
                }

                if (_isBudgetExhausted = _campaignBudget.Budget.GetDouble() <= 0)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_budget_exhausted", Campaign.Name).Inc();
                    await _budgetManager.RequestAdditional(_campaign.Id, EntityType.Campaign);
                    return NO_MATCHES;
                }

                if (_campaign.Schedule == null)
                {
                    BidCounters.NoBidReason.WithLabels("no_campaign_schedule", Campaign.Name).Inc();
                    return NO_MATCHES;
                }

                if (DateTime.UtcNow < _campaign.Schedule.StartDate)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_not_started", Campaign.Name, Campaign.Name).Inc();
                    return NO_MATCHES;
                }

                if (!_campaign.Schedule.EndDate.IsNullOrDefault() && DateTime.UtcNow > _campaign.Schedule.EndDate)
                {
                    BidCounters.NoBidReason.WithLabels("campaign_ended", Campaign.Name).Inc();
                    return NO_MATCHES;
                }

                // Apply campaign filters and check budget
                if (_campaign.IsFiltered(request))
                {
                    BidCounters.NoBidReason.WithLabels("campaign_filtered", Campaign.Name).Inc();
                    return NO_MATCHES;
                }

                var modifier = _campaign.GetModifier(request);

                // Track the bidder cpm
                BidCounters.CampaignBidCPM.WithLabels(Campaign.Name).Observe(modifier);

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
                    BidCounters.NoBidReason.WithLabels("not_all_matched", Campaign.Name).Inc();
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
                var chk = 0L;

                var ret = impList.Select(bidContext =>
                {
                    var cpm = Math.Round(Math.Min(Campaign.MaxCPM, modifier), 6);

                    if (cpm < bidContext.Impression.BidFloor)
                        BidCounters.NoBidReason.WithLabels("bid_too_low", Campaign.Name).Inc();

                    // Verify the limits on cpm
                    if (cpm >= bidContext.Impression.BidFloor && cpm <= Campaign.MaxCPM)
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
                            ImageUrl = bidContext.Content.RawUri,
                            AdId = bidContext.Creative.Id,
                            CreativeId = bidContext.Creative.Id + "." + bidContext.Content.Id,
                            CampaignId = bidContext.Campaign.Id,
                        };
                        bidContext.CPM = cpm;

                        return bidContext;
                    }

                    Interlocked.Increment(ref chk);

                    return null;
                }).Where(b => b != null).ToArray();

                if (chk == 0 && ret.Length == 0)
                    BidCounters.NoBidReason.WithLabels("no_creative_match", Campaign.Name).Inc();

                return ret;
            }
        }

        /// <summary>
        /// Shim method for scoring requests against the campaign
        /// </summary>
        /// <param name="request">The request to score</param>
        /// <returns>A number between -1 and 1</returns>
        double Score(BidRequest request) => _rng.NextDouble() * 2 - 1.0;
    }
}