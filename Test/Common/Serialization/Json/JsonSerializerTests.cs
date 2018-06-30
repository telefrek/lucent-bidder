using System;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json.Test
{
    [TestClass]
    public class JsonSerializerTests : BaseTestClass
    {

        [TestInitialize]
        public override void TestInitialize() => base.TestInitialize();

        [TestMethod]
        public void TestReaderDynamicObject()
        {
            TestContext.Properties.Set("json", @"{""name"":""test""}");
            var reader = ServiceProvider.GetService<ISerializationStreamReader>();

            Assert.IsTrue(reader.HasNext(), "reader should have data");
            Assert.AreEqual(SerializationToken.Object, reader.Token, "Invalid token type");

            var dObj = reader.ReadDynamic();
            Assert.IsNotNull(dObj, "Object should exist");
            Assert.AreEqual("test", dObj.name, false, "Invalid name");
        }

        [TestMethod]
        public void TestReaderNestedDynamicObject()
        {
            TestContext.Properties.Set("json", "{\"name\":\"test\",\"typeinfo\":{\"type\":\"unit\"}}");
            var reader = ServiceProvider.GetService<ISerializationStreamReader>();

            Assert.IsTrue(reader.HasNext(), "reader should have data");
            Assert.AreEqual(SerializationToken.Object, reader.Token, "Invalid token type");

            var dObj = reader.ReadDynamic();
            Assert.IsNotNull(dObj, "Object should exist");
            Assert.AreEqual("test", dObj.name, false, "Invalid name");
            var dObj2 = dObj.typeinfo;
            Assert.IsNotNull(dObj2, "Nested object should exist");
            Assert.AreEqual("unit", dObj2.type, "Type was not serialized correctly");
        }

        [TestMethod]
        public void TestWriterDynamicObject()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            dynamic dObj = new ExpandoObject();
            dObj.name = "test";

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                Assert.IsNotNull(writer, "Writer should have been created");
                writer.Write(dObj);
            }

            Assert.IsTrue(ms.Position > 0, "Memory stream was not affected");
            ms.Seek(0, SeekOrigin.Begin);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            Assert.IsTrue("{\"name\":\"test\"}".Equals(json, StringComparison.InvariantCulture), "Invalid json serialization");
        }

        [TestMethod]
        public void TestWriterNestedDynamicObject()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            dynamic dObj = new ExpandoObject();
            dObj.name = "test";

            dynamic dObj2 = new ExpandoObject();
            dObj2.type = "unit";

            dObj.typeinfo = dObj2;

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                Assert.IsNotNull(writer, "Writer should have been created");
                writer.Write(dObj);
            }

            Assert.IsTrue(ms.Position > 0, "Memory stream was not affected");
            ms.Seek(0, SeekOrigin.Begin);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            Assert.IsTrue("{\"name\":\"test\",\"typeinfo\":{\"type\":\"unit\"}}".Equals(json, StringComparison.InvariantCulture), "Invalid json serialization");
        }

        [TestMethod]
        public async Task TestWriterNestedDynamicObjectAsync()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            dynamic dObj = new ExpandoObject();
            dObj.name = "test";

            dynamic dObj2 = new ExpandoObject();
            dObj2.type = "unit";

            dObj.typeinfo = dObj2;

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                Assert.IsNotNull(writer, "Writer should have been created");
                await writer.WriteAsync(dObj);
            }

            Assert.IsTrue(ms.Position > 0, "Memory stream was not affected");
            ms.Seek(0, SeekOrigin.Begin);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            Assert.IsTrue("{\"name\":\"test\",\"typeinfo\":{\"type\":\"unit\"}}".Equals(json, StringComparison.InvariantCulture), "Invalid json serialization");
        }

        [TestMethod]
        public async Task TestReaderWriterNestedDynamicObjectAsync()
        {
            var ms = new MemoryStream();
            TestContext.Properties.Set("memory.stream", ms);

            dynamic dObj = new ExpandoObject();
            dObj.name = "test";

            dynamic dObj2 = new ExpandoObject();
            dObj2.type = "unit";

            dObj.typeinfo = dObj2;

            using (var writer = ServiceProvider.GetService<ISerializationStreamWriter>())
            {
                Assert.IsNotNull(writer, "Writer should have been created");
                await writer.WriteAsync(dObj);
            }

            Assert.IsTrue(ms.Position > 0, "Memory stream was not affected");
            ms.Seek(0, SeekOrigin.Begin);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            Assert.IsTrue("{\"name\":\"test\",\"typeinfo\":{\"type\":\"unit\"}}".Equals(json, StringComparison.InvariantCulture), "Invalid json serialization");

            TestContext.Properties["json"] = json;
            var reader = ServiceProvider.GetService<ISerializationStreamReader>();

            Assert.IsTrue(await reader.HasNextAsync(), "reader should have data");
            Assert.AreEqual(SerializationToken.Object, reader.Token, "Invalid token type");

            dObj = await reader.ReadDynamicAsync();
            Assert.IsNotNull(dObj, "Object should exist");
            Assert.AreEqual("test", dObj.name, false, "Invalid name");
            dObj2 = dObj.typeinfo;
            Assert.IsNotNull(dObj2, "Nested object should exist");
            Assert.AreEqual("unit", dObj2.type, "Type was not serialized correctly");
        }

        [TestMethod]
        public void TestReaderEmptyDynamicObject()
        {
            TestContext.Properties.Set("json", @"{}");
            var reader = ServiceProvider.GetService<ISerializationStreamReader>();

            Assert.IsTrue(reader.HasNext(), "reader should have data");
            Assert.AreEqual(SerializationToken.Object, reader.Token, "Invalid token type");

            var dObj = reader.ReadDynamic();
            Assert.IsNotNull(dObj, "Object should exist");
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddTransient<ISerializationStreamReader, JsonSerializationStreamReader>((provider) =>
            {
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(TestContext.Properties["json"] as string));
                var jr = new JsonTextReader(new StreamReader(ms));

                return provider.CreateInstance<JsonSerializationStreamReader>(jr);
            });

            services.AddTransient<ISerializationStreamWriter, JsonSerializationStreamWriter>((provider) =>
            {
                var jw = new JsonTextWriter(new StreamWriter(TestContext.Properties["memory.stream"] as Stream, Encoding.UTF8, 4096, true));

                return provider.CreateInstance<JsonSerializationStreamWriter>(jw);
            });
        }
    }
}