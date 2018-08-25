using System;
using System.Threading.Tasks;
using Lucent.Common.Storage;
using Lucent.Portal.Data;
using Lucent.Portal.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Lucent.Portal.Models
{
    public class CreateCampaignModel : PageModel
    {
        private readonly ILucentRepository<Campaign, Guid> _db;
        private readonly ILogger _log;

        public CreateCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log)
        {
            _db = db.GetRepository<Campaign, Guid>();
            _log = log;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            _log.LogInformation("Trying post campaign");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Set the Id
            Campaign.Id = Guid.NewGuid();

            if (await _db.TryInsert(Campaign, (o) => o.Id))
                return RedirectToPage("./Index");

            _log.LogError("Failed to insert record");

            return Page();
        }
    }
}