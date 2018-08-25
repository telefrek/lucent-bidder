using System;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Storage.Test
{
    [TestClass]
    public class StorageTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();

            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            Assert.IsNotNull(manager, "Failed to create manager");

            var testRepo = manager.GetRepository<CTest, Guid>();
            Assert.IsNotNull(testRepo, "Failed to create repo");

            foreach(var entry in testRepo.Get().Result)
                Assert.IsTrue(testRepo.TryRemove(entry, (e)=>e.Id).Result);
        } 

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddStorage(Configuration);
        }

        public class CTest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime LastUpdated { get; set; } = DateTime.Now;
        }

        [TestMethod]
        public async Task TestCassandra()
        {
            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            Assert.IsNotNull(manager, "Failed to create manager");

            var testRepo = manager.GetRepository<CTest, Guid>();
            Assert.IsNotNull(testRepo, "Failed to create repo");

            var res = await testRepo.Get(Guid.NewGuid());
            Assert.IsNull(res, "No object should be returned");
            var tObj = new CTest { Id = Guid.NewGuid(), Name = "item", LastUpdated = DateTime.UtcNow };

            var success = await testRepo.TryInsert(tObj, (o) => o.Id);
            Assert.IsTrue(success);

            res = await testRepo.Get(tObj.Id);
            Assert.IsNotNull(res);
            Assert.AreEqual(tObj.Id, res.Id, "mismatch id");

            tObj.Name = "Updated";

            success = await testRepo.TryUpdate(tObj, (o) => o.Id);
            Assert.IsTrue(success, "Failed to update");

            res = await testRepo.Get(tObj.Id);
            Assert.IsNotNull(res);
            Assert.AreEqual(tObj.Id, res.Id, "mismatch id");
            Assert.AreEqual("Updated", res.Name, true, "Failed to update name");

            success = await testRepo.TryRemove(tObj, (o) => o.Id);
            Assert.IsTrue(success, "Failed to remove");

            res = await testRepo.Get(tObj.Id);
            Assert.IsNull(res, "No object should be returned");
        }
    }
}