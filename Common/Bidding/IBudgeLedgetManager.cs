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
        /// Get the current budget
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        Task<Budget> GetBudgetAsync(string campaignId);

        /// <summary>
        /// Request additional funds
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        Task<Budget> RequestBudgetAsync(string campaignId);
    }

    /// <summary>
    /// 
    /// </summary>
    public class BudgetLedgerManager : IBudgetLedgerManager
    {
        BudgetLedgerConfig _config;
        IStorageManager _manager;
        IStorageRepostory<BudgetLedger> _ledger;
        IStorageRepostory<Campaign> _campaigns;
        IStorageRepostory<Budget> _budgets;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageManager"></param>
        /// <param name="options"></param>
        public BudgetLedgerManager(IStorageManager storageManager, IOptions<BudgetLedgerConfig> options)
        {
            _manager = storageManager;
            _config = options.Value;

            _ledger = _manager.GetRepository<BudgetLedger>();
            _campaigns = _manager.GetRepository<Campaign>();
            _budgets = _manager.GetRepository<Budget>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task<Budget> GetBudgetAsync(string campaignId)
            => await _budgets.Get(campaignId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task<Budget> RequestBudgetAsync(string campaignId)
        {
            var budget = await _budgets.Get(campaignId);
            var camp = await _campaigns.Get(campaignId);
            var entry = await _ledger.Get(campaignId);

            // Verify all components were retrieved correctly
            if (entry != null && camp != null && budget != null)
            {
                // Check for a day rollover
                if (DateTime.Now.Day != entry.LastAllocation.Day)
                {
                    entry.LifetimeTotal = entry.DailyTotal + entry.HourlyTotal;
                    entry.DailyTotal = 0;
                    entry.HourlyTotal = 0;
                }

                // Check for an hour rollover
                else if (DateTime.Now.Subtract(entry.LastAllocation).Hours > 1 || DateTime.Now.Hour != entry.LastAllocation.Hour)
                {
                    entry.DailyTotal += entry.HourlyTotal;
                    entry.HourlyTotal = 0;
                }

                // Update the hourly total
                if (entry.HourlyTotal < camp.SpendCaps.HourlySpendCap && (entry.HourlyTotal + entry.DailyTotal) < camp.SpendCaps.DailySpendCap)
                {
                    // Get the budget remainder for the hour/daily totals
                    var rem = Math.Min(camp.SpendCaps.HourlySpendCap - entry.HourlyTotal, 
                        camp.SpendCaps.DailySpendCap - entry.HourlyTotal - entry.DailyTotal);

                    // Get the min between configuration and remainder
                    rem = Math.Min(rem, _config.AllocationAmount);

                    // Update the amount and allocation time
                    entry.HourlyTotal += rem;
                    entry.LastAllocation = DateTime.Now;

                    // Verify we updated the entry successfully
                    if (await _ledger.TryUpdate(entry))
                    {
                        budget.Remaining += rem;
                        await _budgets.TryUpdate(budget);
                    }
                }
            }

            return budget;
        }
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