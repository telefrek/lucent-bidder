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
        /// <param name="contents"></param>
        /// <returns></returns>
        public static async Task LoadExchange(this Exchange exchange, IServiceProvider provider, byte[] contents)
        {
            using (var ms = new MemoryStream(contents))
            {
                var asm = AssemblyLoadContext.Default.LoadFromStream(ms);
                if (asm != null)
                {
                    var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(AdExchange).IsAssignableFrom(t));
                    if (exchgType != null)
                    {
                        var exchg = provider.CreateInstance(exchgType) as AdExchange;
                        if (exchg != null)
                        {
                            exchg.ExchangeId = (Guid)(object)exchange.Id; // <-- this is proof something is ugly with this code...
                            await exchg.Initialize(provider);
                            (exchange as Exchange).Instance = exchg;
                        }
                    }
                }
            }
        }
    }
}