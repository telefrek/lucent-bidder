using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;
using Lucent.Common.Storage;
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
            var campaigns = ServiceProvider.GetRequiredService<IStorageManager>().GetRepository<Campaign>();
            var camp = new Campaign
            {
                BidFilter = new BidFilter
                {
                    GeoFilters = new Filters.Filter[]
                    {
                        new Filter
                        {
                            Property = "Country",
                            Value = "USA"
                        }
                    }
                },
                Creatives = new List<Creative>()
                {
                    new Creative
                    {
                        Id = "crid1",
                        Contents = new List<CreativeContent>()
                        {
                            new CreativeContent
                            {
                                CanScale = false,
                                H = 100,
                                W = 100,
                                ContentType = ContentType.Banner,
                                MimeType = "image/png",
                            },
                        }
                    }
                }
            };
            camp.IsFiltered = camp.BidFilter.GenerateCode();
            foreach (var c in camp.Creatives.SelectMany(cc => cc.Contents))
                c.HydrateFilter();

            Assert.IsTrue(campaigns.TryInsert(camp).Result, "Failed to insert campaign");
            Assert.AreEqual(0, registry.Exchanges.Count, "No exchanges should be loaded yet");

            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            var target = Path.Combine(Directory.GetCurrentDirectory(), "SimpleExchange.dll");
            Assert.IsTrue(File.Exists(target), "Corrupt test environment");

            var newPath = Path.Combine(exchangePath, "SimpleExchange.dll");
            File.Copy(target, newPath);

            Thread.Sleep(2000);
            Assert.AreEqual(1, registry.Exchanges.Count, "New exchange should have been loaded");

            var response = registry.Exchanges[0].Bid(new BidRequest
            {
                Id = SequentialGuid.NextGuid().ToString(),
                Impressions = new Impression[]
                {
                    new Impression
                    {
                        ImpressionId = "1",
                        Banner = new Banner
                        {
                            H = 100,
                            W = 100,
                            MimeTypes = new string[]{"image/png"}
                        }
                    }
                }
            }).Result;

            if (response == null)
                TestContext.WriteLine("well damn");

            File.Delete(newPath);

            Thread.Sleep(2000);
            Assert.AreEqual(0, registry.Exchanges.Count, "No exchanges should be loaded now");
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddSingleton<IStorageManager, InMemoryStorage>();
            services.AddSingleton<IExchangeRegistry, ExchangeRegistry>().AddBidding(Configuration);
        }
    }
}