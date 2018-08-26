using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Lucent.Common.Serialization.Json;
using System;
using Lucent.Portal.Data;
using Microsoft.EntityFrameworkCore;

namespace Lucent.Portal.Hubs
{
    public class CampaignHub : Hub
    {
        public async Task UpdateCampaignAsync(Campaign campaign, CancellationToken token)
        {
            var sb = new StringBuilder();
            using (var jWriter = new JsonTextWriter(new StringWriter(sb)))
            {
                jWriter.WriteStartObject();
                await jWriter.WritePropertyAsync("id", campaign.Id.ToString());
                await jWriter.WritePropertyAsync("name", campaign.Name);
                await jWriter.WritePropertyAsync("spend", campaign.Spend);
                jWriter.WriteEndObject();
            }

            await Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
        }
    }

    public interface ICampaignUpdateContext
    {
        Task UpdateCampaignSpendAsync(Guid campaignId, double spend, CancellationToken token);
        Task UpdateCampaignAsync(Campaign campaign, CancellationToken token);
    }

    public class CampaignUpdateContext : ICampaignUpdateContext
    {
        readonly IHubContext<CampaignHub> _context;
        readonly PortalDbContext _dbContext;

        public CampaignUpdateContext(IHubContext<CampaignHub> hubContext, PortalDbContext dbContext)
        {
            _context = hubContext;
            _dbContext = dbContext;
        }

        public async Task UpdateCampaignSpendAsync(Guid campaignId, double spend, CancellationToken token)
        {
            var campaign = await _dbContext.Campaigns.FindAsync(campaignId);
            if (campaign != null)
            {
                campaign.Spend += spend;
                _dbContext.Attach(campaign).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

                var sb = new StringBuilder();
                using (var jWriter = new JsonTextWriter(new StringWriter(sb)))
                {
                    jWriter.WriteStartObject();
                    await jWriter.WritePropertyAsync("id", campaign.Id);
                    await jWriter.WritePropertyAsync("spend", campaign.Spend.ToString("0.###"));
                    jWriter.WriteEndObject();
                }

                await _context.Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
            }
        }

        public async Task UpdateCampaignAsync(Campaign campaign, CancellationToken token)
        {
            var sb = new StringBuilder();
            using (var jWriter = new JsonTextWriter(new StringWriter(sb)))
            {
                jWriter.WriteStartObject();
                await jWriter.WritePropertyAsync("id", campaign.Id);
                await jWriter.WritePropertyAsync("name", campaign.Name);
                await jWriter.WritePropertyAsync("spend", campaign.Spend);
                jWriter.WriteEndObject();
            }

            await _context.Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
        }
    }
}