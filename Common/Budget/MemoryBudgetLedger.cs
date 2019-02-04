using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Entities;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// In memory version for testing
    /// </summary>
    public class MemoryBudgetLedger : IBidLedger
    {
        ConcurrentDictionary<string, List<BidEntry>> _ledgers = new ConcurrentDictionary<string, List<BidEntry>>();

        /// <inheritdoc/>
        public Task<bool> TryRecordEntry(string ledgerId, BidEntry source)
        {
            _ledgers.AddOrUpdate(ledgerId, new List<BidEntry>(), (i, l) => l).Add(source);
            return Task.FromResult(true);
        }
    }
}