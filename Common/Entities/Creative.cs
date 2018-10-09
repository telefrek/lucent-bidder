using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Filters;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    public class Creative : IStorageEntity
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        public List<CreativeContent> Contents { get; set; } = new List<CreativeContent>();
        string IStorageEntity.ETag { get; set; }
        DateTime IStorageEntity.Updated { get; set; }
    }
}