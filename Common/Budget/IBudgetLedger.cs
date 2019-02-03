using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Stores bid history
    /// </summary>
    public interface IBudgetLedger
    {
        /// <summary>
        /// Tries to record an entry with the source and amount
        /// </summary>
        Task<bool> TryRecordEntry<T>(string ledgerId, T source, EntityType eType, double amount) where T : class, new();
    }
}