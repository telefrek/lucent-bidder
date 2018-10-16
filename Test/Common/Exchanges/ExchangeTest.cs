using System.IO;
using System.Threading;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Exchanges
{
    [TestClass]
    public class ExchangeTest : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            if (Directory.Exists(exchangePath))
                Directory.Delete(exchangePath, true);
            Directory.CreateDirectory(exchangePath);

            base.TestInitialize();
        }

        [TestMethod]
        public void TestExchangeLifecycle()
        {
            var registry = ServiceProvider.GetRequiredService<IExchangeRegistry>();

            Assert.AreEqual(0, registry.Exchanges.Count, "No exchanges should be loaded yet");

            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            var target = Path.Combine(Directory.GetCurrentDirectory(), "SimpleExchange.dll");
            Assert.IsTrue(File.Exists(target), "Corrupt test environment");

            var newPath = Path.Combine(exchangePath, "SimpleExchange.dll");
            File.Copy(target, newPath);

            Thread.Sleep(2000);
            Assert.AreEqual(1, registry.Exchanges.Count, "New exchange should have been loaded");

            File.Delete(newPath);

            Thread.Sleep(2000);
            Assert.AreEqual(0, registry.Exchanges.Count, "No exchanges should be loaded now");
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddSingleton<IExchangeRegistry, ExchangeRegistry>().AddBidding(Configuration);
        }
    }
}