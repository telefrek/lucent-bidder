using System;
using System.IO;
using System.Text;
using Lucent.Common.OpenRTB;
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

            var serialStream = ms.WrapSerializer(ServiceProvider, SerializationFormat.JSON, true);
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

            var serialStream = ms.WrapSerializer(ServiceProvider, SerializationFormat.JSON, true);
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

        [TestMethod]
        public void TestStreamGeneric()
        {
            var ms = new MemoryStream();

            var serialStream = ms.WrapSerializer(ServiceProvider, SerializationFormat.JSON, true);
            Assert.IsTrue(ms.Position == 0, "Serialization stream should leave the base stream alone");

            var geo = new Geo
            {
                Latitude = 1.0,
                Longitude = 0.1,
                GeoType = GeoType.GPS,
                Accuracy = 1,
                LastFixed = 1,
                ISP = ISP.MaxMind,
                Country = "USA",
                Region = "California",
                Metro = "South Bay",
                City = "Santa Clara",
                Zip = "95050",
                UtcOffset = -7,
            };

            using (var writer = serialStream.Writer)
            {
                writer.Write(geo);
            }

            // This call should NOT fail
            ms.Seek(0, SeekOrigin.Begin);

            var contents = Encoding.UTF8.GetString(ms.ToArray());

            using (var reader = serialStream.Reader)
            {
                Assert.IsTrue(reader.HasNext(), "Reader should have more objects");
                var g = reader.ReadAs<Geo>();
                Assert.IsNotNull(g);
            }
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddTransient<ISerializationRegistry, SerializationRegistry>();
        }
    }
}