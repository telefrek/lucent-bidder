using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

            using (var reader = ServiceProvider.GetService<ISerializationStreamReader>())
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

        [TestMethod]
        public void TestEntitySerializer()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            ServiceProvider.GetService<ISerializationRegistry>().Register<TestObject>(new TestObjectSerializer());

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                var tObj = new TestObject
                {
                    Age = 1,
                    Name = "Test",
                    Keywords = new string[] { "Key1", "Key 2" },
                };

                var prop = new ProtobufProperty { PropertyIndex = 0, Type = WireType.LEN_ENCODED };
                writer.Write(prop);
                writer.Write(tObj);
            }

            Assert.IsTrue(ms.Length > 0, "No bytes written to stream");
            TestContext.Properties.Set("bytes", ms.ToArray());

            using (var reader = ServiceProvider.GetService<ISerializationStreamReader>())
            {
                Assert.IsTrue(reader.HasNext(), "Failed to read object");
                var obj = reader.ReadAs<TestObject>();

                Assert.IsNotNull(obj, "Failed to read test object");
                Assert.AreEqual(1, obj.Age, "Wrong age");
                Assert.AreEqual("Test", obj.Name, false, "Invalid string");
                Assert.IsNotNull(obj.Keywords, "Missing keywords");
                Assert.IsTrue(obj.Keywords.Length > 0, "Missing keywords");
                Assert.AreEqual(2, obj.Keywords.Length, "Wrong number of keywords");
            }
        }


        [TestMethod]
        public void TestEntityArraySerializer()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            ServiceProvider.GetService<ISerializationRegistry>().Register<TestObject>(new TestObjectSerializer());

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                var objects = new List<TestObject>();

                for (var i = 0; i < 5; ++i)
                    objects.Add(new TestObject
                    {
                        Age = 1,
                        Name = "Test",
                        Keywords = new string[] { "Key1", "Key 2" },
                    });

                var prop = new ProtobufProperty { PropertyIndex = 1, Type = WireType.LEN_ENCODED };
                writer.Write(prop);
                writer.Write(objects.ToArray());
            }

            Assert.IsTrue(ms.Length > 0, "No bytes written to stream");
            TestContext.Properties.Set("bytes", ms.ToArray());

            using (var reader = ServiceProvider.GetService<ISerializationStreamReader>())
            {
                Assert.IsTrue(reader.HasNext(), "Failed to read object");
                var arr = reader.ReadAsArray<TestObject>();

                Assert.IsNotNull(arr, "Array was null");
                Assert.IsTrue(arr.Length > 0, "Array was empty");
                Assert.AreEqual(5, arr.Length, "Not all objects deserialized");

                foreach (var obj in arr)
                {
                    Assert.IsNotNull(obj, "Failed to read test object");
                    Assert.AreEqual(1, obj.Age, "Wrong age");
                    Assert.AreEqual("Test", obj.Name, false, "Invalid string");
                    Assert.IsNotNull(obj.Keywords, "Missing keywords");
                    Assert.IsTrue(obj.Keywords.Length > 0, "Missing keywords");
                    Assert.AreEqual(2, obj.Keywords.Length, "Wrong number of keywords");
                }
            }
        }

        public class TestObject
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string[] Keywords { get; set; }
        }

        public class TestObjectSerializer : IEntitySerializer<TestObject>
        {
            public TestObject Read(ISerializationStreamReader serializationStreamReader)
            {
                var tObj = new TestObject();
                while (serializationStreamReader.HasNext())
                {
                    var obj = serializationStreamReader.ReadAs<ProtobufProperty>();
                    switch (obj.PropertyIndex)
                    {
                        case 0UL:
                            tObj.Age = serializationStreamReader.ReadInt();
                            break;
                        case 1UL:
                            tObj.Name = serializationStreamReader.ReadString();
                            break;
                        case 2UL:
                            tObj.Keywords = serializationStreamReader.ReadStringArray();
                            break;
                        default:
                            if (serializationStreamReader.Token != SerializationToken.EndOfStream)
                                Assert.Fail("Invalid state");
                            break;
                    }
                };

                return tObj;
            }

            public Task<TestObject> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
            {
                serializationStreamReader.Token.Guard(SerializationToken.Object);

                return null;
            }

            public void Write(ISerializationStreamWriter serializationStreamWriter, TestObject instance)
            {
                var prop = new ProtobufProperty { PropertyIndex = 0, Type = WireType.VARINT };
                serializationStreamWriter.Write(prop);
                serializationStreamWriter.Write(instance.Age);

                prop = new ProtobufProperty { PropertyIndex = 1, Type = WireType.LEN_ENCODED };
                serializationStreamWriter.Write(prop);
                serializationStreamWriter.Write(instance.Name);

                prop = new ProtobufProperty { PropertyIndex = 2, Type = WireType.LEN_ENCODED };
                serializationStreamWriter.Write(prop);
                serializationStreamWriter.Write(instance.Keywords);
            }

            public Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, TestObject instance, CancellationToken token)
            {
                throw new System.NotImplementedException();
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