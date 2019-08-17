using Prometheus;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Common counters
    /// </summary>
    public class BidCounters
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static Counter NoBidReason = Metrics.CreateCounter("no_bid_reasons", "Reasons the bidder didn't bid", new CounterConfiguration
        {
            LabelNames = new string[] { "reason" }
        });

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static Counter BudgetRequests = Metrics.CreateCounter("budget_requests", "Request information for budgets", new CounterConfiguration
        {
            LabelNames = new string[] { "state" }
        });

        /// <summary>
        /// Track campaign bids
        /// </summary>
        /// <value></value>
        public static Counter CampaignBids = Metrics.CreateCounter("campaign_bids", "Bids per campaign", new CounterConfiguration
        {
            LabelNames = new string[] { "campaign" }
        });

        /// <summary>
        /// Track campaign wins
        /// </summary>
        /// <value></value>
        public static Counter CampaignWins = Metrics.CreateCounter("campaign_wins", "Wins per campaign", new CounterConfiguration
        {
            LabelNames = new string[] { "campaign" }
        });

        /// <summary>
        /// Track campaign clicks for ctr
        /// </summary>
        /// <value></value>
        public static Counter CampaignImpressions = Metrics.CreateCounter("campaign_impressions", "Impressions per campaign", new CounterConfiguration
        {
            LabelNames = new string[] { "campaign" }
        });

        /// <summary>
        /// Track campaign clicks for ctr
        /// </summary>
        /// <value></value>
        public static Counter CampaignClicks = Metrics.CreateCounter("campaign_clicks", "Clicks per campaign", new CounterConfiguration
        {
            LabelNames = new string[] { "campaign" }
        });

        /// <summary>
        /// Track campaign conversions
        /// </summary>
        /// <value></value>
        public static Counter CampaignConversions = Metrics.CreateCounter("campaign_conversions", "Conversions per campaign", new CounterConfiguration
        {
            LabelNames = new string[] { "campaign" }
        });

        /// <summary>
        /// Track campaign spend
        /// </summary>
        /// <value></value>
        public static Counter CampaignSpend = Metrics.CreateCounter("campaign_spend", "Amount of spend per campaign", new CounterConfiguration { LabelNames = new string[] { "campaign" } });


        /// <summary>
        /// Track campaign spend
        /// </summary>
        /// <value></value>
        public static Counter CampaignCPM = Metrics.CreateCounter("campaign_cpm", "Amount of spend per campaign", new CounterConfiguration { LabelNames = new string[] { "campaign" } });

        /// <summary>
        /// Track campaign spend
        /// </summary>
        /// <value></value>
        public static Histogram CampaignBidCPM = Metrics.CreateHistogram("campaign_bid_cpm", "Projection of spend per campaign", new HistogramConfiguration
        {
            LabelNames = new string[] { "campaign" },
            Buckets = new[]{
                0.1, 0.2, 0.3, 0.4, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0, 2.5, 3, 4, 5
            }
        });

        /// <summary>
        /// Track campaign revenue
        /// </summary>
        /// <value></value>
        public static Counter CampaignRevenue = Metrics.CreateCounter("campaign_revenue", "Amount of spend per campaign", new CounterConfiguration { LabelNames = new string[] { "campaign" } });

        /// <summary>
        /// Track device os
        /// </summary>
        /// <value></value>
        public static Counter DeviceOS = Metrics.CreateCounter("device_os", "Device OS", new CounterConfiguration { LabelNames = new string[] { "os" } });


        /// <summary>
        /// Track total bids
        /// </summary>
        /// <value></value>
        public static Counter Bids = Metrics.CreateCounter("total_bids", "Total Bids", new CounterConfiguration { });
    }
}