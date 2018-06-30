using System;
using System.IO;
using System.Text;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Serialization.Test
{
    [TestClass]
    public class SerializationStreamTests : BaseTestClass
    {

        [TestInitialize]
        public override void TestInitialize() => base.TestInitialize();

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestStreamCloseCtor()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"test\"}"));
            ms.Position = 0;

            var serialStream = new SerializationStream(ms, SerializationFormat.JSON, ServiceProvider, false);
            Assert.IsTrue(ms.Position == 0, "Serialization stream should leave the base stream alone");

            using (var reader = serialStream.Reader)
            {
                Assert.IsNotNull(reader, "JsonReader should have been provided");

                Assert.IsTrue(reader.HasNext(), "Reader should have data");
                Assert.AreEqual(reader.Token, SerializationToken.Object, "Object should have been identified");
                reader.Skip();
                Assert.AreEqual(reader.Token, SerializationToken.EndOfStream, "Reader shouldn't have more data");
            }

            // This call should fail
            ms.Seek(0, SeekOrigin.Begin);            
        }

        [TestMethod]
        public void TestStreamOpenCtor()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"test\"}"));
            ms.Position = 0;

            var serialStream = new SerializationStream(ms, SerializationFormat.JSON, ServiceProvider, true);
            Assert.IsTrue(ms.Position == 0, "Serialization stream should leave the base stream alone");

            using (var reader = serialStream.Reader)
            {
                Assert.IsNotNull(reader, "JsonReader should have been provided");

                Assert.IsTrue(reader.HasNext(), "Reader should have data");
                Assert.AreEqual(reader.Token, SerializationToken.Object, "Object should have been identified");
                reader.Skip();
                Assert.AreEqual(reader.Token, SerializationToken.EndOfStream, "Reader shouldn't have more data");
            }

            // This call should NOT fail
            ms.Seek(0, SeekOrigin.Begin);            
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddTransient<ISerializationRegistry>((provider) =>
            {
                return null;
            });
        }
    }
}