using System.Threading.Tasks;
using Lucent.Common.Entities;

namespace Lucent.Common.Api
{
    /// <summary>
    /// Campaign API interface
    /// </summary>
    public interface ICampaignApi
    {
        /// <summary>
        /// Try to add a campaign
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> TryAdd(Campaign instance);

        /// <summary>
        /// Try to update the campaign
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> TryUpdate(Campaign instance);

        /// <summary>
        /// Try to delete the campaign
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> TryDelete(Campaign instance);

        /// <summary>
        /// Get all the campaigns
        /// </summary>
        /// <returns></returns>
        Task<Campaign[]> TryGet();
    }
}