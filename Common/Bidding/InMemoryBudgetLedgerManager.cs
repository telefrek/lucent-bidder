using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Entities;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public class InMemoryBudgetLedgerManager : IBudgetLedgerManager
    {
        IStorageRepository<Campaign> _campaignRepo;
        IStorageRepository<LedgerEntry> _ledger;
        ILogger<InMemoryBudgetLedgerManager> _log;
        BudgetLedgerConfig _config;
        IServiceProvider _provider;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        /// <param name="storageManager"></param>
        /// <param name="config"></param>
        public InMemoryBudgetLedgerManager(IServiceProvider provider, ILogger<InMemoryBudgetLedgerManager> logger, IStorageManager storageManager, IOptions<BudgetLedgerConfig> config)
        {
            _log = logger;
            _campaignRepo = storageManager.GetRepository<Campaign>();
            _ledger = storageManager.GetRepository<LedgerEntry>();
            _config = config.Value;
            _provider = provider;
        }

        /// <inheritdoc/>
        public ICampaignLedger GetLedger(Campaign campaign) => _provider.CreateInstance<CampaignLedger>(campaign, this) as ICampaignLedger;

        /// <inheritdoc/>
        public async Task<LedgerEntry> RequestBudgetAsync(string campaignId)
        {
            _log.LogInformation("Requesting budget for {0}", campaignId);

            // Check if the current campaign is over budget
            var c = await _campaignRepo.Get(new StringStorageKey(campaignId));
            if (c == null)
                return null;

            var current = (await _ledger.GetAny(new LedgerCompositeEntryKey { TargetId = c.Id })).Where(e => e.Created > DateTime.Now.Subtract(TimeSpan.FromDays(1)));

            if (current.Sum(e => e.OriginalAmount) > c.SpendCaps.DailySpendCap)
                return null;

            if (current.Where(e => e.Created > DateTime.Now.Subtract(TimeSpan.FromHours(1))).Sum(e => e.OriginalAmount) > c.SpendCaps.HourlySpendCap)
                return null;

            var amt = Math.Min(_config.AllocationAmount, c.SpendCaps.DailySpendCap - current.Sum(e => e.OriginalAmount));

            var entry = new LedgerEntry
            {
                Key = new LedgerCompositeEntryKey { TargetId = campaignId, LedgerTimeId = TimeUuid.NewId() },
                OriginalAmount = amt,
                RemainingAmount = amt,
            };

            if (await _ledger.TryInsert(entry))
                return entry;

            return null;
        }
    }
}