using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Lucent.Portal.Data;
using Lucent.Common.Entities;
using System;
using Microsoft.Extensions.Logging;
using Lucent.Common.Storage;
using System.Linq;
using Lucent.Common.Messaging;

namespace Lucent.Portal.Models
{
    public class CampaignsModel : PageModel
    {
        private readonly IBasicStorageRepository<Campaign> _db;
        private readonly ILogger<CampaignsModel> _log;
        readonly IMessageFactory _factory;

        public CampaignsModel(IStorageManager db, ILogger<CampaignsModel> logger, IMessageFactory factory)
        {
            _db = db.GetBasicRepository<Campaign>();
            _log = logger;
            _factory = factory;
        }

        [BindProperty]
        public IList<Campaign> Campaigns { get; set; }

        public async Task OnGetAsync()
        {
            _log.LogInformation("Getting campaigns");
            Campaigns = (await _db.GetAll()).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var contact = (await _db.GetAll()).ToList().FirstOrDefault(c => c.Id == id);

            if (contact != null)
            {
                if (await _db.TryRemove(contact))
                {
                    _log.LogInformation("Created campaign, sending across clusters");
                    var cluster = _factory.GetClusters().FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(cluster))
                    {
                        var msg = _factory.CreateMessage<LucentMessage<Campaign>>();
                        msg.Body = contact;
                        msg.Route = contact.Id;
                        msg.ContentType = "application/x-protobuf";
                        msg.Headers.Add("x-lucent-action-type", "delete");

                        if (_factory.CreatePublisher(cluster, "campaign-updates").TryPublish(msg))
                            _log.LogInformation("published message for cross cluster");
                        else
                            _log.LogWarning("Failed to publish to secondary cluster");
                    }
                }
            }

            return RedirectToPage();
        }
    }
}