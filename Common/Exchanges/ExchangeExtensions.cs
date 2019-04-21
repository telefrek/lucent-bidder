using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;

namespace Lucent.Common
{
    public partial class LucentExtensions
    {
        // This is a hack around not loading the same assembly again and again
        static ConcurrentDictionary<string, Assembly> _loadedAssemblies = new ConcurrentDictionary<string, Assembly>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task LoadExchange(this Exchange exchange, IServiceProvider provider)
        {
            try
            {
                using (var ms = exchange.Code)
                {
                    ms.Position = 0;
                    // Hash the exchange code

                    // if already loaded, use that (hard to prevent dual loading in netcore 2.0)
                    var hash = Convert.ToBase64String(SHA256.Create().ComputeHash(ms.ToArray()));
                    var asm = (Assembly)null;
                    if (!_loadedAssemblies.TryGetValue(hash, out asm))
                    {
                        asm = AssemblyLoadContext.Default.LoadFromStream(ms);
                        _loadedAssemblies.AddOrUpdate(hash, asm, (s, a) => asm);
                    }

                    var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(AdExchange).IsAssignableFrom(t));
                    var exchg = provider.CreateInstance(exchgType) as AdExchange;
                    if (exchg != null)
                    {
                        exchg.ExchangeId = exchange.Id;
                        await exchg.Initialize(provider);
                        (exchange as Exchange).Instance = exchg;
                    }
                }
            }
            catch (Exception)
            {
                // nom nom nom
            }
            exchange.Code = null; // clear id
        }
    }
}