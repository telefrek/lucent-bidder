using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public Task<ICollection<LedgerSummary>> TryGetSummary(string entityId, DateTime start, DateTime end, int? numSegments, bool? detailed)
        {
            // This doesn't really work lol
            var entries = new List<BidEntry>();
            var summaries = new List<LedgerSummary>();
            if (_ledgers.TryGetValue(entityId, out entries))
            {
                summaries.Add(new LedgerSummary
                {
                    Start = start,
                    End = end,
                    Bids = entries.Count,
                    Amount = entries.Sum(e => e.Cost),
                });
            }

            return Task.FromResult((ICollection<LedgerSummary>)summaries);
        }

        /// <inheritdoc/>
        public Task<bool> TryRecordEntry(string ledgerId, BidEntry source, Dictionary<string, object> metadata)
        {
            _ledgers.AddOrUpdate(ledgerId, new List<BidEntry>(), (i, l) => l).Add(source);
            return Task.FromResult(true);
        }
    }
}