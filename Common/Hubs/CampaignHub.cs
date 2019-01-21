using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Microsoft.AspNetCore.SignalR;
using Lucent.Common.Serialization.Json;
using Lucent.Common.Serialization;

namespace Lucent.Common.Hubs
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
        readonly ISerializationContext _serializationContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="serializationContext"></param>
        public CampaignUpdateContext(IHubContext<CampaignHub> hubContext, ISerializationContext serializationContext)
        {
            _context = hubContext;
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
            await _context.Clients.All.SendAsync("CampaignUpdate", sb.ToString(), token);
        }
    }
}