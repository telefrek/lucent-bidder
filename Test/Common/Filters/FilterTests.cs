using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
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
    public class FilterTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddSerialization(Configuration);
        }

        [TestMethod]
        public void FilterSandbox()
        {
            var dt = 1.0d;

            var pexp = Expression.Parameter(typeof(Campaign), "campaign");
            var prop = Expression.Property(pexp, "Spend");
            var target = Expression.Constant(dt);
            var method = Expression.Call(prop, "Equals", null, target);
            var lambda = Expression.Lambda<Func<Campaign, bool>>(method, pexp);

            var m = lambda.Compile();

            Assert.IsNotNull(m, "Fail!");
            var c = new Campaign { Spend = dt };
            Assert.IsTrue(m.Invoke(c), "Wrong spend");
        }
    }
}