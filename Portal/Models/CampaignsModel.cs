using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Lucent.Portal.Data;
using Lucent.Common.Entities;
using System;
using Microsoft.Extensions.Logging;
using Lucent.Common.Storage;
using System.Linq;

namespace Lucent.Portal.Models
{
    public class CampaignsModel : PageModel
    {
        private readonly ILucentRepository<Campaign> _db;
        private readonly ILogger<CampaignsModel> _log;

        public CampaignsModel(IStorageManager db, ILogger<CampaignsModel> logger)
        {
            _db = db.GetRepository<Campaign>();
            _log = logger;
        }

        public IList<Campaign> Campaigns { get; private set; }

        public async Task OnGetAsync()
        {
            _log.LogInformation("Getting campaigns");
            Campaigns = (await _db.Get()).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var contact = await _db.Get(id);

            if (contact != null)
            {
                await _db.TryRemove(contact);
            }

            return RedirectToPage();
        }
    }
}