using System;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Storage;
using Lucent.Portal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Lucent.Portal.Models
{
    public class CreateCampaignModel : PageModel
    {
        private readonly ILucentRepository<Campaign> _db;
        private readonly ILogger _log;

        public CreateCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log)
        {
            _db = db.GetRepository<Campaign>();
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
            Campaign.Id = Guid.NewGuid().ToString();

            if (await _db.TryInsert(Campaign))
                return RedirectToPage("./Index");

            _log.LogError("Failed to insert record");

            return Page();
        }
    }
}