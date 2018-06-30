using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lucent.Portal.Entities
{
    public class Creative
    {
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
    }
}