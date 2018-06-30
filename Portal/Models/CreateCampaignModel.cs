using System;
using System.Threading.Tasks;
using Lucent.Portal.Data;
using Lucent.Portal.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lucent.Portal.Models
{
    public class CreateCampaignModel : PageModel
    {
        private readonly PortalDbContext _db;

        public CreateCampaignModel(PortalDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Set the Id
            Campaign.Id = Guid.NewGuid();

            _db.Campaigns.Add(Campaign);
            await _db.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}