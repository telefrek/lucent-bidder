using System;
using System.IO;
using Lucent.Common.Budget;
using Lucent.Common.Exchanges;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Exchange : IStorageEntity
    {
        /// <inheritdoc/>
        [SerializationProperty(1, "id")]
        public Guid Id
        {
            get => (Guid)Key.RawValue()[0];
            set
            {
                Key = new GuidStorageKey(value);
            }
        }

        /// <inheritdoc/>
        public StorageKey Key { get; set; } = new GuidStorageKey(SequentialGuid.NextGuid());

        /// <inheritdoc/>
        public int Version { get; set; }

        /// <inheritdoc/>
        public string ETag { get; set; }

        /// <inheritdoc/>
        public DateTime Updated { get; set; }

        /// <summary>
        /// Exchange name
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "name")]
        public string Name { get; set; }

        /// <summary>
        /// Track the last code update
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "codeUpdated")]
        public DateTime LastCodeUpdate { get; set; }

        /// <summary>
        /// Budget scheduling for the exchange
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "budgetSchedule")]
        public BudgetSchedule BudgetSchedule { get; set; }

        /// <summary>
        /// CampaignIds that have been added to this exchange
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "campaigns")]
        public String[] CampaignIds { get; set; }


        /// <summary>
        /// CampaignIds that have been added to this exchange
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Instance code if loaded
        /// </summary>
        /// <value></value>
        public AdExchange Instance { get; set; }

        /// <summary>
        /// Code if loading from db
        /// </summary>
        /// <value></value>
        public MemoryStream Code { get; set; }

        /// <inheritdoc/>
        public EntityType EntityType { get; set; } = EntityType.Exchange;
    }
}