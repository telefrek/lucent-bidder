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
using Lucent.Common.Serialization;

namespace Lucent.Portal.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    public class CampaignHub : Hub
    {
        readonly ISerializationContext _serializationContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializationContext"></param>
        public CampaignHub(ISerializationContext serializationContext)
        {
            _serializationContext = serializationContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateCampaignAsync(Campaign campaign, CancellationToken token)
        {
            var sb = new StringBuilder();
            await _serializationContext.Write(await new StringWriter(sb).CreateJsonObjectWriter(JsonFormat.Normal), campaign);
            await Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignUpdateContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="spend"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task UpdateCampaignSpendAsync(Guid campaignId, double spend, CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task UpdateCampaignAsync(Campaign campaign, CancellationToken token);
    }

    /// <summary>
    /// 
    /// </summary>
    public class CampaignUpdateContext : ICampaignUpdateContext
    {
        readonly IHubContext<CampaignHub> _context;
        readonly PortalDbContext _dbContext;
        readonly ISerializationContext _serializationContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="serializationContext"></param>
        /// <param name="dbContext"></param>
        public CampaignUpdateContext(IHubContext<CampaignHub> hubContext, ISerializationContext serializationContext, PortalDbContext dbContext)
        {
            _context = hubContext;
            _dbContext = dbContext;
            _serializationContext = serializationContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="spend"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateCampaignSpendAsync(Guid campaignId, double spend, CancellationToken token)
        {
            var campaign = await _dbContext.Campaigns.FindAsync(campaignId);
            if (campaign != null)
            {
                campaign.Spend += spend;
                _dbContext.Attach(campaign).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

                var sb = new StringBuilder();
                await _serializationContext.Write(await new StringWriter(sb).CreateJsonObjectWriter(JsonFormat.Normal), campaign);
                await _context.Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateCampaignAsync(Campaign campaign, CancellationToken token)
        {
            var sb = new StringBuilder();
            await _serializationContext.Write(await new StringWriter(sb).CreateJsonObjectWriter(JsonFormat.Normal), campaign);
            await _context.Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
        }
    }
}