using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Lucent.Portal.Data;
using Lucent.Portal.Entities;
using System;
using Microsoft.Extensions.Logging;

namespace Lucent.Portal.Models
{
    public class CampaignsModel : PageModel
    {
        private readonly PortalDbContext _db;
        private readonly ILogger<CampaignsModel> _log;

        public CampaignsModel(PortalDbContext db, ILogger<CampaignsModel> logger)
        {
            _db = db;
            _log = logger;
        }

        public IList<Campaign> Campaigns { get; private set; }

        public async Task OnGetAsync()
        {
            _log.LogInformation("Getting campaigns");
            Campaigns = await _db.Campaigns.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var contact = await _db.Campaigns.FindAsync(id);

            if (contact != null)
            {
                _db.Campaigns.Remove(contact);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}