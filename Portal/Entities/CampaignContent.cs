using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lucent.Portal.Entities
{
    /// <summary>
    /// Simple wrapper for keeping a campaign aware of the existing creatives
    /// </summary>
    public class CampaignContent
    {
        public List<Creative> CreativeCatalog { get; set; }
        public Campaign Campaign { get; set; }
    }
}