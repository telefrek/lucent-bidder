using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lucent.Portal.Entities
{
    public class CampaignFilters
    {
        public string FilterTypeName { get; set; }
        
        [Display(Name = "Filter Types")]
        public IEnumerable<SelectListItem> FilterTypes { get; set; } = new SelectList(Enum.GetNames(typeof(FilterType)).Select(fn => new SelectListItem { Value = fn, Text = fn }).ToList(), "Value", "Text");
    }
}