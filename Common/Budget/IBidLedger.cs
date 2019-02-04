using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Stores bid history
    /// </summary>
    public interface IBidLedger
    {
        /// <summary>
        /// Tries to record an entry with the source and amount
        /// </summary>
        Task<bool> TryRecordEntry(string ledgerId, BidEntry source);
    }
}