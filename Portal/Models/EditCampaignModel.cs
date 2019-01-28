using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Storage;
using Lucent.Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using Lucent.Common.Hubs;

namespace Lucent.Portal.Models
{
    public class EditCampaignModel : PageModel
    {
        private readonly IStorageRepository<Campaign> _campaignDb;

        private readonly IStorageRepository<Creative> _creativeDb;
        private readonly ILogger _log;
        private readonly ICampaignUpdateContext _context;
        private readonly IMessageFactory _factory;

        public EditCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log, ICampaignUpdateContext context, IMessageFactory factory)
        {
            _campaignDb = db.GetRepository<Campaign>();
            _creativeDb = db.GetRepository<Creative>();
            _log = log;
            _context = context;
            _factory = factory;
        }

        [BindProperty]
        public Campaign Campaign { get; set; }

        [BindProperty]
        public List<Creative> CreativeCatalog { get; set; }

        public string FilterTypeValue { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            _log.LogInformation("Geting campaign {id}", id);
            var c = await _campaignDb.Get(new StringStorageKey(id));
            CreativeCatalog = (await _creativeDb.GetAll()).ToList();

            if (c != null)
            {
                Campaign = c;
                return Page();
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostRemoveCreativeAsync(string id)
        {
            _log.LogInformation("Removing creative {id}", id);
            var creative = await _creativeDb.Get(new StringStorageKey(id));
            CreativeCatalog = (await _creativeDb.GetAll()).ToList();
            if(creative != null)
            {
                //Campaign.Creatives.Remove(creative);
                _log.LogInformation("Updating campaign");
                if(await _campaignDb.TryUpdate(Campaign))
                {
                    _log.LogInformation("Success");
                    Campaign = await _campaignDb.Get(Campaign.Key);
                }
                else
                    _log.LogWarning("Failed to update campaign");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddCreativeAsync(string id)
        {
            _log.LogInformation("Adding creative {id}", id);
            var creative = await _creativeDb.Get(new StringStorageKey(id));
            CreativeCatalog = (await _creativeDb.GetAll()).ToList();
            if(creative != null)
            {
                //Campaign.Creatives.Add(creative);
                _log.LogInformation("Updating campaign");
                if(await _campaignDb.TryUpdate(Campaign))
                {
                    _log.LogInformation("Success");
                    Campaign = await _campaignDb.Get(Campaign.Key);
                }
                else
                    _log.LogWarning("Failed to update campaign");
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

            var c = await _campaignDb.Get(Campaign.Key);
            if (c != null)
            {
                try
                {
                    if (await _campaignDb.TryUpdate(Campaign))
                    {
                        await _context.UpdateCampaignAsync(Campaign, CancellationToken.None);

                        var cluster = _factory.GetClusters().FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(cluster))
                        {
                            var msg = _factory.CreateMessage<LucentMessage<Campaign>>();
                            msg.Body = Campaign;
                            msg.Route = Campaign.Id;
                            msg.ContentType = "application/x-protobuf";
                            msg.Headers.Add("x-lucent-action-type", "update");

                            if (await _factory.CreatePublisher(cluster, "campaign-updates").TryPublish(msg))
                                _log.LogInformation("published message for cross cluster");
                            else
                                _log.LogWarning("Failed to publish to secondary cluster");
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new Exception($"Campaign {Campaign.Key} not found!");
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