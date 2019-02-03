using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lucent.Common.Entities;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// In memory version for testing
    /// </summary>
    public class MemoryBudgetLedger : IBudgetLedger
    {
        ConcurrentDictionary<string, Tuple<object, EntityType, double>> _ledgers = new ConcurrentDictionary<string, Tuple<object, EntityType, double>>();

        /// <inheritdoc/>
        public Task<bool> TryRecordEntry<T>(string ledgerId, T source, EntityType eType, double amount) where T : class, new() => Task.FromResult(_ledgers.TryAdd(ledgerId, new Tuple<object, EntityType, double>(source, eType, amount)));
    }
}