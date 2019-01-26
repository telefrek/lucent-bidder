using System.Threading.Tasks;
using Lucent.Common.Storage;
using Lucent.Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Lucent.Common.Hubs;

namespace Lucent.Portal.Models
{
    public class SummaryCampaignModel : PageModel
    {
        private readonly IBasicStorageRepository<Campaign> _db;
        private readonly ILogger _log;
        private readonly ICampaignUpdateContext _context;

        public SummaryCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log, ICampaignUpdateContext context)
        {
            _db = db.GetBasicRepository<Campaign>();
            _log = log;
            _context = context;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            _log.LogInformation("Geting campaign {id}", id);
            var c = await _db.Get(id);

            if (c != null)
            {
                Campaign = c;
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}