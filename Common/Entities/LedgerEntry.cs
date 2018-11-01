using System;
using Cassandra;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Individual ledger entry
    /// </summary>
    public class LedgerEntry : IStorageEntity<LedgerCompositeEntryKey>
    {
        /// <summary>
        /// Composite key for identifying a ledger
        /// </summary>
        /// <returns></returns>
        public LedgerCompositeEntryKey Id { get; set; } = new LedgerCompositeEntryKey();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public LedgerEntryType EntryType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double OriginalAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double RemainingAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ETag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime Updated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Key type for retrieving ledger entries
    /// </summary>
    public class LedgerCompositeEntryKey
    {
        /// <summary>
        /// The id for the target type
        /// </summary>
        /// <value></value>
        public string TargetId { get; set; }

        /// <summary>
        /// The TimeUUID for storing the entry
        /// </summary>
        /// <value></value>
        public Guid LedgerTimeId { get; set; }

        public override bool Equals(object obj)
        {
            var entry = obj as LedgerCompositeEntryKey;
            if (entry == null) return false;

            if (entry.LedgerTimeId == null) return entry.TargetId == TargetId;

            return entry.TargetId == TargetId && entry.LedgerTimeId == entry.LedgerTimeId;
        }
    }
}