using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Storage;
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
        private readonly ILucentRepository<Campaign, Guid> _db;
        private readonly ILogger _log;
        private readonly ICampaignUpdateContext _context;

        public EditCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log, ICampaignUpdateContext context)
        {
            _db = db.GetRepository<Campaign, Guid>();
            _log = log;
            _context = context;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            _log.LogInformation("Geting campaign {id}");
            var c = await _db.Get(id);

            if (Campaign == null)
            {
                Campaign = c;
                Campaign.Creatives.Add(new Creative { Name = "Colleen", Id = Guid.NewGuid() });
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _log.LogInformation("Validating post");
            if (!ModelState.IsValid || Campaign == null)
            {
                return Page();
            }

            var c = await _db.Get(Campaign.Id);
            if (c != null)
            {
                c.Name = Campaign.Name;


                try
                {
                    if (await _db.TryUpdate(c, (o) => c.Id))
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