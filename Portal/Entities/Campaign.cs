using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lucent.Portal.Entities
{
    public class Campaign
    {
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        public double Spend { get; set; }

        public List<Creative> Creatives { get; set; } = new List<Creative>();
    }
}