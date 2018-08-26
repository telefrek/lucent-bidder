using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    public class Campaign : IStorageEntity
    {
        [Required, StringLength(100)]
        public string Name { get; set; }
        public double Spend { get; set; }

        public List<Creative> Creatives { get; set; } = new List<Creative>();
        public string Id { get; set; }
        string IStorageEntity.ETag { get; set; }
        DateTime IStorageEntity.Updated { get; set; }
        int IStorageEntity.Version { get; set; }
    }
}