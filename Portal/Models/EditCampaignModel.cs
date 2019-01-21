using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Storage;
using Lucent.Portal.Data;
using Lucent.Common.Entities;
using Lucent.Portal.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Lucent.Common.Messaging;
using Microsoft.Extensions.Logging;
using System.Linq;
using Lucent.Portal.Entities;
using System.Collections.Generic;

namespace Lucent.Portal.Models
{
    public class EditCampaignModel : PageModel
    {
        private readonly IBasicStorageRepository<Campaign> _campaignDb;

        private readonly IBasicStorageRepository<Creative> _creativeDb;
        private readonly ILogger _log;
        private readonly ICampaignUpdateContext _context;
        private readonly IMessageFactory _factory;

        public EditCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log, ICampaignUpdateContext context, IMessageFactory factory)
        {
            _campaignDb = db.GetBasicRepository<Campaign>();
            _creativeDb = db.GetBasicRepository<Creative>();
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
            var c = await _campaignDb.Get(id);
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
            var creative = await _creativeDb.Get(id);
            CreativeCatalog = (await _creativeDb.GetAll()).ToList();
            if(creative != null)
            {
                //Campaign.Creatives.Remove(creative);
                _log.LogInformation("Updating campaign");
                if(await _campaignDb.TryUpdate(Campaign))
                {
                    _log.LogInformation("Success");
                    Campaign = await _campaignDb.Get(Campaign.Id);
                }
                else
                    _log.LogWarning("Failed to update campaign");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddCreativeAsync(string id)
        {
            _log.LogInformation("Adding creative {id}", id);
            var creative = await _creativeDb.Get(id);
            CreativeCatalog = (await _creativeDb.GetAll()).ToList();
            if(creative != null)
            {
                //Campaign.Creatives.Add(creative);
                _log.LogInformation("Updating campaign");
                if(await _campaignDb.TryUpdate(Campaign))
                {
                    _log.LogInformation("Success");
                    Campaign = await _campaignDb.Get(Campaign.Id);
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

            var c = await _campaignDb.Get(Campaign.Id);
            if (c != null)
            {
                try
                {
                    if (await _campaignDb.TryUpdate(Campaign))
                    {
                        await _context.UpdateCampaignAsync(Campaign, CancellationToken.None);

                        _log.LogInformation("Modified campaign, sending across clusters");
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