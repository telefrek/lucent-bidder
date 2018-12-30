using System;
using Lucent.Common.Exchanges;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Exchange : IStorageEntity<Guid>
    {
        /// <inheritdoc/>
        public Guid Id { get; set; }

        /// <inheritdoc/>
        public string ETag { get; set; }

        /// <inheritdoc/>
        public DateTime Updated { get; set; }

        /// <summary>
        /// Instance code if loaded
        /// </summary>
        /// <value></value>
        public IAdExchange Instance { get; set; }
    }
}