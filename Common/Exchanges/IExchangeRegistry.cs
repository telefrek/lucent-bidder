using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Registry of known exchanges
    /// </summary>
    public interface IExchangeRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        AdExchange GetExchange(HttpContext context);
    }
}