using System;
using System.Threading.Tasks;
using Lucent.Portal.Data;
using Lucent.Portal.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lucent.Portal.Models
{
    public class EditCampaignModel : PageModel
    {
        private readonly PortalDbContext _db;

        public EditCampaignModel(PortalDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Campaign = await _db.Campaigns.FindAsync(id);

            Campaign.Creatives.Add(new Creative { Name = "Colleen", Id = Guid.NewGuid()});

            if (Campaign == null)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _db.Attach(Campaign).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception($"Campaign {Campaign.Id} not found!");
            }

            return RedirectToPage("./Index");
        }
    }
}