using System;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Represents Bidder global rules
    /// </summary>
    public class BidderFilter : IStorageEntity
    {
        /// <inheritdoc/>
        [SerializationProperty(1, "id")]
        public string Id
        {
            get => Key.ToString();
            set
            {
                Key = new StringStorageKey(value);
            }
        }

        /// <inheritdoc/>
        public StorageKey Key { get; set; } = new StringStorageKey();

        /// <inheritdoc/>
        public int Version {get;set;}

        /// <summary>
        /// Filters for incoming bids before exchange evaluation
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "bidfilter")]
        public BidFilter BidFilter { get; set; }

        /// <inheritdoc/>
        public string ETag { get; set; }

        /// <inheritdoc/>
        public DateTime Updated { get; set; }


        /// <inheritdoc/>
        public EntityType EntityType { get; set; } = EntityType.BidderFilter;
    }
}