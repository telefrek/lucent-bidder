using System;
using System.Threading.Tasks;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBudgetCache
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<double> TryUpdateBudget(string key, double value);
    }
}