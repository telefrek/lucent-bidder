using System;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Storage;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Interface for managing budgets per campaign
    /// </summary>
    public interface IBudgetLedgerManager
    {
        /// <summary>
        /// Request additional funds
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        Task<LedgerEntry> RequestBudgetAsync(string campaignId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaign"></param>
        /// <returns></returns>
        ICampaignLedger GetLedger(Campaign campaign);
    }

    /// <summary>
    /// Configuration POCO for bdget management
    /// </summary>
    public class BudgetLedgerConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string LedgerUri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double AllocationAmount { get; set; } = 5d; // Amount of budget to request each time
    }
}