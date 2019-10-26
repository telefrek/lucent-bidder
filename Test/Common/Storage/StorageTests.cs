using System;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Storage.Test
{
    [TestClass]
    public class StorageTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            Assert.IsNotNull(manager, "Failed to create manager");

            var testRepo = manager.GetRepository<Creative>();
            Assert.IsNotNull(testRepo, "Failed to create repo");

            foreach (var entry in testRepo.GetAll().Result)
                Assert.IsTrue(testRepo.TryRemove(entry).Result);
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddLucentServices(Configuration, localOnly: true);
        }

        [TestMethod]
        public async Task TestCassandra()
        {
            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            Assert.IsNotNull(manager, "Failed to create manager");

            var testRepo = manager.GetRepository<Creative>();
            Assert.IsNotNull(testRepo, "Failed to create repo");

            var res = await testRepo.Get(new StringStorageKey(Guid.NewGuid().ToString()));
            Assert.IsNull(res, "No object should be returned");
            var tObj = new Creative { Id = Guid.NewGuid().ToString(), Name = "item" };

            tObj.Contents = new CreativeContent[]{
                new CreativeContent {
                CreativeUri = "https://google.com/mycreative",
                ContentLocation = "/mnt/somepath",
                RawUri = "https://google.com/rawcreative",
                Duration = 100
            }};

            var success = await testRepo.TryInsert(tObj);
            Assert.IsTrue(success);

            res = await testRepo.Get(tObj.Key);
            Assert.IsNotNull(res);
            Assert.AreEqual(tObj.Key, res.Id, "mismatch id");
            Assert.AreEqual(tObj.Contents.Length, 1, "mismatch count");
            Assert.AreEqual(tObj.Contents[0].Duration, 100, "bad duration");

            tObj.Name = "Updated";

            success = await testRepo.TryUpdate(tObj);
            Assert.IsTrue(success, "Failed to update");

            res = await testRepo.Get(tObj.Key);
            Assert.IsNotNull(res);
            Assert.AreEqual(tObj.Key, res.Id, "mismatch id");
            Assert.AreEqual("Updated", res.Name, true, "Failed to update name");

            success = await testRepo.TryRemove(tObj);
            Assert.IsTrue(success, "Failed to remove");

            res = await testRepo.Get(tObj.Key);
            Assert.IsNull(res, "No object should be returned");
        }
    }
}