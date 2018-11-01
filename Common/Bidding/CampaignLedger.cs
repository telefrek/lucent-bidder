using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Default implementation
    /// </summary>
    public class CampaignLedger : ICampaignLedger
    {
        IStorageRepository<Campaign, string> _campaignRepo;
        IStorageRepository<LedgerEntry, LedgerCompositeEntryKey> _ledger;
        ILogger _log;
        IBudgetLedgerManager _ledgerManager;
        LedgerEntry _currentEntry;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="c"></param>
        /// <param name="budgetLedgerManager"></param>
        /// <param name="logger"></param>
        /// <param name="storageManager"></param>
        public CampaignLedger(Campaign c, IBudgetLedgerManager budgetLedgerManager, ILogger logger, IStorageManager storageManager)
        {
            Campaign = c;
            _log = logger;
            _campaignRepo = storageManager.GetRepository<Campaign, string>();
            _ledger = storageManager.GetRepository<LedgerEntry, LedgerCompositeEntryKey>();
            _ledgerManager = budgetLedgerManager;

            _currentEntry = _ledger.Get(new LedgerCompositeEntryKey { TargetId = c.Id }).Result;
        }

        /// <inheritdoc/>
        public Campaign Campaign { get; set; }

        /// <inheritdoc/>
        public async Task<bool> CheckSpend(double amount)
        {
            if (_currentEntry == null || _currentEntry.RemainingAmount == 0)
            {
                if (await GetAdditionalFundsAsync())
                    return _currentEntry.RemainingAmount > 0;
            }
            else
                return _currentEntry.RemainingAmount > 0;

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> GetAdditionalFundsAsync()
        {
            _currentEntry = await _ledgerManager.RequestBudgetAsync(Campaign.Id);
            if (_currentEntry != null && _currentEntry.RemainingAmount > 0)
                return true;

            return false;
        }
    }
}