using System;
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

        /// <summary>
        /// Initialize the registry
        /// </summary>
        /// <returns></returns>
        Task Initialize();

        /// <summary>
        /// Check for the exchange
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasExchange(Guid id);
    }
}