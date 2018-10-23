using System;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Represents Bidder global rules
    /// </summary>
    public class BidderFilter : IStorageEntity
    {
        /// <summary>
        /// Filters for incoming bids before exchange evaluation
        /// </summary>
        /// <value></value>
        public BidFilter BidFilter { get; set; }

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