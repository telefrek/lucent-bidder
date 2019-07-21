using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Filters;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Creative : IStorageEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
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
        public int Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Required, StringLength(100)]
        [SerializationProperty(2, "name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "title")]
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "desc")]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SerializationProperty(5, "contents")]
        public CreativeContent[] Contents { get; set; }

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
        public EntityType EntityType { get; set; } = EntityType.Creative;
    }
}