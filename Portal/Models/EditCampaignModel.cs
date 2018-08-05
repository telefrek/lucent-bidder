using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Portal.Data;
using Lucent.Portal.Entities;
using Lucent.Portal.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lucent.Portal.Models
{
    public class EditCampaignModel : PageModel
    {
        private readonly PortalDbContext _db;
        private readonly ILogger<EditCampaignModel> _log;
        private readonly ICampaignUpdateContext _context;

        public EditCampaignModel(PortalDbContext db, ILogger<EditCampaignModel> logger, ICampaignUpdateContext context)
        {
            _db = db;
            _log = logger;
            _context = context;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            _log.LogInformation("Geting campaign {id}");
            Campaign = await _db.Campaigns.FindAsync(id);

            Campaign.Creatives.Add(new Creative { Name = "Colleen", Id = Guid.NewGuid() });

            if (Campaign == null)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _log.LogInformation("Validating post");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var c = (Campaign)await _db.FindAsync(typeof(Campaign), Campaign.Id);
            if(c != null)
            {
                c.Name = Campaign.Name;
                _db.Attach(c).State = EntityState.Modified;

                try
                {
                    await _db.SaveChangesAsync();
                    await _context.UpdateCampaignAsync(c, CancellationToken.None);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new Exception($"Campaign {Campaign.Id} not found!");
                }
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError("", "Model no longer exists");
                return Page();
            }

        }
    }
}