using System;
using System.IO;
using Lucent.Common.Exchanges;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Exchange : IStorageEntity<Guid>
    {
        /// <inheritdoc/>
        [SerializationProperty(1, "id")]
        public Guid Id { get; set; }

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
        /// Instance code if loaded
        /// </summary>
        /// <value></value>
        public AdExchange Instance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public MemoryStream Code { get; set; }

        /// <inheritdoc/>
        public EntityType EntityType { get; set; } = EntityType.Exchange;
    }
}