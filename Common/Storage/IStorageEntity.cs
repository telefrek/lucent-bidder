using System;
using Lucent.Common.Entities;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Represents a storage entity
    /// </summary>
    /// <typeparam name="K">The type of key</typeparam>
    public interface IStorageEntity<K>
    {
        /// <summary>
        /// The unique identifier for the object
        /// </summary>
        /// <value></value>
        K Id { get; set; }

        /// <summary>
        /// The object ETag for detecting invalid changes
        /// </summary>
        /// <value></value>
        string ETag { get; set; }

        /// <summary>
        /// The timestamp for the last time the object was modified
        /// </summary>
        /// <value></value>
        DateTime Updated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        EntityType EntityType { get; set; }
    }
}