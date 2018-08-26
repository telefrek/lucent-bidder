using System;
using System.Collections.Generic;
using System.IO;
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
    public class SerializationTests : BaseTestClass
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
        public async Task TestProtoSerializer()
        {
            var dt = DateTime.Now;

            using(var ms = new MemoryStream())
            {
                using(var writer = ms.WrapSerializer(ServiceProvider, SerializationFormat.PROTOBUF, true).Writer)
                {
                    await writer.WriteAsync(dt);
                    await writer.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);

                using(var reader = ms.WrapSerializer(ServiceProvider, SerializationFormat.PROTOBUF, true).Reader)
                {
                    var v1 = reader.ReadDateTime();
                    Assert.AreEqual(dt, v1, "date mismatch");
                }
            }

            using(var ms = new MemoryStream())
            {
                using(var writer = ms.WrapSerializer(ServiceProvider, SerializationFormat.JSON, true).Writer)
                {
                    await writer.WriteAsync(dt);
                    await writer.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);

                using(var reader = ms.WrapSerializer(ServiceProvider, SerializationFormat.JSON, true).Reader)
                {
                    var v1 = reader.ReadDateTime();
                    Assert.AreEqual(dt, v1, "date mismatch");
                }
            }
        }
    }
}