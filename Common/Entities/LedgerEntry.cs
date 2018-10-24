using System;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Individual ledger entry
    /// </summary>
    public class LedgerEntry : IClusteredStorageEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string SecondaryId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public LedgerEntryType EntryType {get;set;}

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
        public string Id { get; set; }

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
    }
}