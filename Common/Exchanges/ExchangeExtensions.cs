using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;

namespace Lucent.Common
{
    public partial class LucentExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task LoadExchange(this Exchange exchange, IServiceProvider provider)
        {
            using (var ms = exchange.Code)
            {
                var asm = AssemblyLoadContext.Default.LoadFromStream(ms);
                var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(AdExchange).IsAssignableFrom(t));
                var exchg = provider.CreateInstance(exchgType) as AdExchange;
                if (exchg != null)
                {
                    exchg.ExchangeId = exchange.Id;
                    await exchg.Initialize(provider);
                    (exchange as Exchange).Instance = exchg;
                }
            }
            exchange.Code = null; // clear id
        }
    }
}