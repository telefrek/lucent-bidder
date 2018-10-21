using System.Collections.Generic;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// Registry of known exchanges
    /// </summary>
    public interface IExchangeRegistry
    {
        /// <summary>
        /// Gets the current set of exchanges
        /// </summary>
        /// <value></value>
        List<IAdExchange> Exchanges { get; }
    }
}