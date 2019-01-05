using System;
using Cassandra;
using Lucent.Common.Serialization;
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
        [SerializationProperty(1, "key")]
        public LedgerCompositeEntryKey Id { get; set; } = new LedgerCompositeEntryKey();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "entrytype")]
        public LedgerEntryType EntryType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "original")]
        public double OriginalAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "remaining")]
        public double RemainingAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

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

        /// <inheritdoc/>
        public EntityType EntityType { get; set; } = EntityType.Ledger;
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
        [SerializationProperty(1, "targetid")]
        public string TargetId { get; set; }

        /// <summary>
        /// The TimeUUID for storing the entry
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "ledgerid")]
        public Guid LedgerTimeId { get; set; }

        /// <summary>
        /// Override equality comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var entry = obj as LedgerCompositeEntryKey;
            if (entry == null) return false;

            return entry.LedgerTimeId == null ? entry.TargetId == TargetId :
                entry.TargetId == TargetId && entry.LedgerTimeId == entry.LedgerTimeId;
        }

        /// <summary>
        /// Override so warnings shut up
        /// </summary>
        public override int GetHashCode()
        {
            return LedgerTimeId == null ? TargetId.GetHashCode() :
                LedgerTimeId.GetHashCode();
        }
    }
}