using System;
using Cassandra;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Individual ledger entry
    /// </summary>
    public class LedgerEntry : IStorageEntity
    {
        /// <summary>
        /// Composite key for identifying a ledger
        /// </summary>
        /// <returns></returns>
        [SerializationProperty(1, "key")]
        public LedgerCompositeEntryKey Id
        {
            get => Key as LedgerCompositeEntryKey;
            set
            {
                Key = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IStorageKey Key { get; set; } = new LedgerCompositeEntryKey();

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
    public class LedgerCompositeEntryKey : IStorageKey
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

        /// <inheritdoc/>
        public override int GetHashCode() => (TargetId + (LedgerTimeId == default(Guid) ? Guid.Empty : LedgerTimeId).ToString()).GetHashCode();

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            var lck = obj as LedgerCompositeEntryKey;

            if (lck != null)
            {
                var cmp = TargetId.CompareTo(lck.TargetId);
                return cmp == 0 ? LedgerTimeId == lck.LedgerTimeId ? 0 : cmp : cmp;
            }

            return -1;
        }

        /// <inheritdoc/>
        public override string ToString() => "{0}:{1}".FormatWith(TargetId, LedgerTimeId);

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

        /// <inheritdoc/>
        public void Parse(string value)
        {
            var data = value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length > 2)
                throw new NotSupportedException();
            TargetId = data[0];
            if (data.Length > 1)
                LedgerTimeId = Guid.Parse(data[1]);
        }

        /// <inheritdoc/>
        public object[] RawValue() => new object[] { TargetId, LedgerTimeId };
    }
}