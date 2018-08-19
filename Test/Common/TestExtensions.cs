using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.OpenRTB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Test
{
    [TestClass]
    public class TestExtensions : BaseTestClass
    {

        [TestInitialize]
        public override void TestInitialize() => base.TestInitialize();

        protected override void InitializeDI(IServiceCollection services)
        {
        }

        [TestMethod]
        public async Task TestExtensionsLoading()
        {
            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Directory.GetCurrentDirectory() + "/" + @"Utils.dll");
            foreach(var ctype in myAssembly.GetTypes())
            {
                if(typeof(ILucentExtension).IsAssignableFrom(ctype))
                {
                    var ext = (ILucentExtension)myAssembly.CreateInstance(ctype.FullName);
                    if(ext != null)
                        ext.Load(ServiceProvider, Configuration);
                }
            }

            var br = new BidRequest();
            var re = await ExchangeRepository.Instance.BidAsync(br);

            Assert.IsNotNull(re, "response was not loaded");
        }
    }
}