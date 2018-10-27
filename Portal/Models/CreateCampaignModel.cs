using System;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Messaging;
using Lucent.Common.Storage;
using Lucent.Portal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Lucent.Portal.Models
{
    public class CreateCampaignModel : PageModel
    {
        private readonly IStorageRepository<Campaign> _db;
        private readonly ILogger _log;
        private readonly IMessageFactory _factory;

        public CreateCampaignModel(IStorageManager db, ILogger<CreateCampaignModel> log, IMessageFactory factory)
        {
            _db = db.GetRepository<Campaign>();
            _log = log;
            Campaign = new Campaign { Schedule = new CampaignSchedule() };
            _factory = factory;
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
            {
                _log.LogInformation("Created campaign, sending across clusters");
                var cluster = _factory.GetClusters().FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(cluster))
                {
                    var msg = _factory.CreateMessage<LucentMessage<Campaign>>();
                    msg.Body = Campaign;
                    msg.Route = Campaign.Id;
                    msg.ContentType = "application/x-protobuf";
                    msg.Headers.Add("x-lucent-action-type", "create");

                    if (_factory.CreatePublisher(cluster, "campaign-updates").TryPublish(msg))
                        _log.LogInformation("published message for cross cluster");
                    else
                        _log.LogWarning("Failed to publish to secondary cluster");
                }
                return RedirectToPage("./Index");
            }

            _log.LogError("Failed to insert record");

            return Page();
        }
    }
}