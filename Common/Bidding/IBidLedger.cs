using System.Threading.Tasks;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Stores bid history
    /// </summary>
    public interface IBidLedger
    {
        /// <summary>
        /// Store a bid
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="bid"></param>
        /// <returns></returns>
        Task<bool> TryRecordBid(string entityId, Bid bid);
    }
}