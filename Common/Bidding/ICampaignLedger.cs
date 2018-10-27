using System.Threading.Tasks;
using Lucent.Common.Entities;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignLedger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        Campaign Campaign { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<bool> CheckSpend(double amount);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> GetAdditionalFundsAsync();
    }
}