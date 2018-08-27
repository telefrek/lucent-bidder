using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
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

        class Filter
        {
            public string Target { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }

        [TestMethod]
        public void FilterSandbox()
        {
            var tfilter = new Filter { Target = "Lucent.Common.OpenRTB.Geo", Property = "Country", Value = "USA" };

            // build our filter

            Type cls = null;

            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                cls = asm.GetType(tfilter.Target, false, true);
                if (cls != null)
                    break;
            }
            Assert.IsNotNull(cls, "bad form peter");

            var param1 = Expression.Parameter(cls, "p1");
            var prop1 = Expression.Property(param1, tfilter.Property);

            var eFilter = Expression.Equal(prop1, Expression.Constant(tfilter.Value));
            var ftype = typeof(Func<,>).MakeGenericType(cls, typeof(bool));

            var lmaker = typeof(Expression).GetMethods().Where(m =>
                m.Name == "Lambda" && m.IsGenericMethod && m.GetGenericArguments().Length == 1
                ).First().MakeGenericMethod(ftype);

            var comp = lmaker.Invoke(null, new object[] { eFilter, new ParameterExpression[] { param1 } });

            var lam = comp.GetType().GetMethod("Compile", Type.EmptyTypes);
            dynamic l1 = lam.Invoke(comp, new object[] { });

            Assert.IsTrue(l1.Invoke(new Geo { Country = "USA" }), "Wrong country");
        }
    }
}