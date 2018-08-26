using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Entities;
using Lucent.Common.Serialization;
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
            ServiceProvider.AddEntitySerializers();

            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            Assert.IsNotNull(manager, "Failed to create manager");

            var testRepo = manager.GetRepository<Campaign>();
            Assert.IsNotNull(testRepo, "Failed to create repo");

            foreach (var entry in testRepo.Get().Result)
                Assert.IsTrue(testRepo.TryRemove(entry).Result);
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddSerialization(Configuration);
            services.AddStorage(Configuration);
        }

        [TestMethod]
        public async Task TestCassandra()
        {
            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            Assert.IsNotNull(manager, "Failed to create manager");

            var testRepo = manager.GetRepository<Campaign>();
            Assert.IsNotNull(testRepo, "Failed to create repo");

            var res = await testRepo.Get(Guid.NewGuid().ToString());
            Assert.IsNull(res, "No object should be returned");
            var tObj = new Campaign { Id = Guid.NewGuid().ToString(), Name = "item", Spend = 1.0 };

            var success = await testRepo.TryInsert(tObj);
            Assert.IsTrue(success);

            res = await testRepo.Get(tObj.Id);
            Assert.IsNotNull(res);
            Assert.AreEqual(tObj.Id, res.Id, "mismatch id");

            tObj.Name = "Updated";

            success = await testRepo.TryUpdate(tObj);
            Assert.IsTrue(success, "Failed to update");

            res = await testRepo.Get(tObj.Id);
            Assert.IsNotNull(res);
            Assert.AreEqual(tObj.Id, res.Id, "mismatch id");
            Assert.AreEqual("Updated", res.Name, true, "Failed to update name");

            success = await testRepo.TryRemove(tObj);
            Assert.IsTrue(success, "Failed to remove");

            res = await testRepo.Get(tObj.Id);
            Assert.IsNull(res, "No object should be returned");
        }
    }
}