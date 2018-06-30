using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Lucent.Portal.Data;
using Lucent.Portal.Entities;
using System;

namespace Lucent.Portal.Models
{
    public class CampaignsModel : PageModel
    {
        private readonly PortalDbContext _db;

        public CampaignsModel(PortalDbContext db)
        {
            _db = db;
        }

        public IList<Campaign> Campaigns { get; private set; }

        public async Task OnGetAsync()
        {
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