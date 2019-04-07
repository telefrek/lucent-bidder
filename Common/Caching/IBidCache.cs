using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// Cache bids
    /// </summary>
    public interface IBidCache
    {
        /// <summary>
        /// Get the bid entry by it's id
        /// </summary>
        /// <param name="id">The entry id</param>
        /// <returns></returns>
        Task<BidResponse> getEntryAsync(string id);

        /// <summary>
        /// Save the set of bid entries
        /// </summary>
        /// <param name="response">The entries to save</param>
        /// <returns></returns>
        Task saveEntries(BidResponse response);
    }
}