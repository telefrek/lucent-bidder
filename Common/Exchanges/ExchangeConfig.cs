using System.IO;

namespace Lucent.Common.Exchanges
{
    /// <summary>
    /// POCO configuration for exchange loading
    /// </summary>
    public class ExchangeConfig
    {
        /// <summary>
        /// Exchange watch location
        /// </summary>
        /// <returns></returns>
        public string ExchangeLocation { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
    }
}