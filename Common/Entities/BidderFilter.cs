using System;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Represents Bidder global rules
    /// </summary>
    public class BidderFilter : IBasicStorageEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Filters for incoming bids before exchange evaluation
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "bidfilter")]
        public BidFilter BidFilter { get; set; }

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
        public EntityType EntityType { get; set; } = EntityType.BidderFilter;
    }
}