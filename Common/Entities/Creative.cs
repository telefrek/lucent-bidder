using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Filters;
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
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CreativeContent> Contents { get; set; } = new List<CreativeContent>();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        string IStorageEntity.ETag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        DateTime IStorageEntity.Updated { get; set; }
    }
}