using System.Dynamic;
using System.IO;
using Lucent.Common.Protobuf;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Serialization.Protobuf.Test
{

    [TestClass]
    public class ProtobufSerializerTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize() => base.TestInitialize();

        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void TestWriteDynamicObject()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            dynamic dObj = new ExpandoObject();
            dObj.name = "test";

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                writer.Write(dObj);
                Assert.Fail("Expected a serialization exception");
            }
        }

        [TestMethod]
        public void TestReadWriteSingleProperty()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                var prop = new ProtobufProperty
                {
                    PropertyIndex = 2,
                    Type = WireType.VARINT
                };

                writer.Write(prop);
                writer.Write(123);
            }

            var bytes = ms.ToArray();
            Assert.IsTrue(bytes.Length > 0, "No bytes written to memory stream!");
            Assert.AreEqual(2, bytes.Length, "Wrong number of bytes written");

            TestContext.Properties.Set("bytes", bytes);

            using(var reader = ServiceProvider.GetService<ISerializationStreamReader>())
            {
                Assert.IsTrue(reader.HasNext(), "Stream was empty");
                var prop = reader.ReadAs<ProtobufProperty>();
                Assert.AreEqual(SerializationToken.Value, reader.Token, "Invalid token after read");
                Assert.AreEqual(WireType.VARINT, prop.Type, "Invalid wire type");
                Assert.AreEqual(2UL, prop.PropertyIndex, "Invalid index");

                var val = reader.ReadInt();
                Assert.AreEqual(123, val, "Invalid value read from the stream");
                Assert.IsFalse(reader.HasNext());
                Assert.AreEqual(SerializationToken.EndOfStream, reader.Token, "Failed to notice end of stream");
            }
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddTransient<ISerializationStreamReader, ProtobufSerializationStreamReader>((provider) =>
            {
                var ms = new MemoryStream((byte[])TestContext.Properties["bytes"]);
                var pr = new ProtobufReader(ms);

                return provider.CreateInstance<ProtobufSerializationStreamReader>(pr);
            });

            services.AddTransient<ISerializationStreamWriter, ProtobufSerializationStreamWriter>((provider) =>
            {
                var jw = new ProtobufWriter(TestContext.Properties["memory.stream"] as Stream, true);

                return provider.CreateInstance<ProtobufSerializationStreamWriter>(jw);
            });
        }
    }
}