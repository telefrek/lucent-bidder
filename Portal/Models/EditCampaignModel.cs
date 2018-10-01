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

namespace Lucent.Portal.Models
{
    public class EditCampaignModel : PageModel
    {
        private readonly ILucentRepository<Campaign> _db;
        private readonly ILogger _log;
        private readonly ICampaignUpdateContext _context;
        private readonly IMessageFactory _factory;

        public EditCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log, ICampaignUpdateContext context, IMessageFactory factory)
        {
            _db = db.GetRepository<Campaign>();
            _log = log;
            _context = context;
            _factory = factory;
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
                    if (await _db.TryUpdate(c))
                    {
                        await _context.UpdateCampaignAsync(c, CancellationToken.None);

                        _log.LogInformation("Modified campaign, sending across clusters");
                        var cluster = _factory.GetClusters().FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(cluster))
                        {
                            var msg = _factory.CreateMessage<LucentMessage<Campaign>>();
                            msg.Body = Campaign;
                            msg.ContentType = "application/x-protobuf";
                            msg.Headers.Add("x-lucent-action-type", "update");

                            if (_factory.CreatePublisher(cluster, "campaign-updates").TryPublish(msg))
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