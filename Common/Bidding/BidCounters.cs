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
    }
}