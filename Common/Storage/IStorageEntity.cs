using System;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Represents a storage compliant entity
    /// </summary>
    public interface IStorageEntity
    {
        /// <summary>
        /// The unique identifier for the object
        /// </summary>
        /// <value></value>
        string Id { get; set; }
        
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
    }
}