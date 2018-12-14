using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;
using Lucent.Common.Serialization.Protobuf;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
            services.AddLucentServices(Configuration, localOnly: true);
        }


        class TestObj
        {
            [SerializationProperty(1, "geo")]
            public Geo Geo { get; set; }

            [SerializationProperty(2, "boo")]
            public string[] Arr { get; set; }

            [SerializationProperty(3, "gooeyduck")]
            public string[] Arr2 { get; set; }
        }

        [TestMethod]
        public async Task TestAsyncGeneration()
        {
            var geo = new Geo
            {
                Country = "country",
                Accuracy = 1,
                City = "city",
                Region = "region",
                RegionFips = "fips",
                GeoType = GeoType.IPAddress,
                Metro = "metro",
                ISP = ISP.Neustar,
                Latitude = 1,
                Longitude = 1,
                Zip = "zip",
                UtcOffset = 1,
                LastFixed = 1,
            };

            var test = new TestObj { Geo = geo, Arr = new[] { "Test1", "Test2" }, Arr2 = new[] { "Hello", "Colleen" } };

            var serializationContext = ServiceProvider.GetRequiredService<ISerializationContext>();

            using (var ms = new MemoryStream())
            {
                await serializationContext.WriteTo(test, ms, true, SerializationFormat.JSON);

                ms.Seek(0, SeekOrigin.Begin);
                var tmp1 = Encoding.UTF8.GetString(ms.ToArray());
                TestContext.WriteLine("Serialized as : {0}", tmp1);

                var testGeo = await serializationContext.ReadFrom<TestObj>(ms, false, SerializationFormat.JSON);

                var tmp = JsonConvert.SerializeObject(test);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                await serializationContext.WriteTo(test, ms, true, SerializationFormat.JSON);

                ms.Seek(0, SeekOrigin.Begin);

                var testGeo = (await serializationContext.ReadFrom<TestObj>(ms, false, SerializationFormat.JSON)).Geo;

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                await serializationContext.WriteTo(test, ms, true, SerializationFormat.PROTOBUF);

                ms.Seek(0, SeekOrigin.Begin);

                var testGeo = (await serializationContext.ReadFrom<TestObj>(ms, false, SerializationFormat.PROTOBUF)).Geo;

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            foreach (var fmt in new[] { SerializationFormat.PROTOBUF, SerializationFormat.JSON })
                for (var i = 0; i < 25; ++i)
                {
                    var sw = new Stopwatch();
                    using (var ms = new MemoryStream())
                    {
                        sw.Start();
                        await serializationContext.WriteTo(test, ms, true, fmt);
                        sw.Stop();

                        ms.Seek(0, SeekOrigin.Begin);
                        var tmp1 = Encoding.UTF8.GetString(ms.ToArray());

                        sw.Start();
                        var testGeo = await serializationContext.ReadFrom<TestObj>(ms, false, fmt);
                        sw.Stop();

                        var tmp = JsonConvert.SerializeObject(test);
                        Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
                    }
                    TestContext.WriteLine("Serialized {0} in {1:#,##0.000} ms", fmt, sw.ElapsedTicks * 1000d / Stopwatch.Frequency);
                }
        }

        [TestMethod]
        public async Task TestDeserializeBid()
        {
            var context = ServiceProvider.GetService<ISerializationContext>();
            var rawBid = @"{""id"":""c6987c2b-edb4-4b7b-b8cf-157af1d485e3"",""site"":{""id"":""gumgum_www.answers.com_ed2265d8"",""ref"":""http://ad32.answers.com/click.php?source=fb&param4=fb-us-de-red&param3=www.answers.com%2Farticle%2F31029589%2Finsanely-useful-life-hacks-to-make-everything-easier&param1=tattoo&param2=67660042&param5=10153631993521186&param6=6049542139960&adt=4342"",""publisher"":{""name"":""www.answers.com"",""id"":""gumgum_946353442_12535""},""name"":""www.answers.com"",""cat"":[""IAB24""],""domain"":""answers.com"",""ext"":{},""page"":""http://www.answers.com/article/31029589/insanely-useful-life-hacks-to-make-everything-easier?paramt=null&param4=fb-us-de-red&param1=tattoo&param2=67660042&s=8""},""wseat"":[""165"",""16""],""source"":{""fd"":0},""user"":{""id"":""5e29eb00-c30a-416e-9d2a-2e18901f0916"",""ext"":{""cookie_age"":64,""consent"":""Y29uc2VudCBkYXRh""},""buyeruid"":""CAESEHL-9O4oJOAiC1Y0O2EHTcE""},""device"":{""pxratio"":0,""language"":""en"",""mccmnc"":""310-005"",""w"":1920,""geo"":{""country"":""US"",""lon"":-80.237,""city"":""West Palm Beach"",""lat"":26.638,""zip"":""33414"",""region"":""FL"",""type"":2},""os"":""Windows"",""devicetype"":2,""h"":1080,""ip"":""73.139.39.18"",""js"":1,""ua"":""Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0"",""dnt"":0},""tmax"":75,""cur"":[""USD""],""imp"":[{""bidfloor"":3.213,""metric"":[{""type"":""viewability"",""value"":0.85}],""id"":""1"",""banner"":{""pos"":1,""h"":600,""battr"":[1,3,5,6,8,9,10,14,15,16],""w"":160,""format"":[{""h"":300,""w"":300},{""h"":350,""w"":300}],""btype"":[1]},""exp"":300,""tagid"":""gumgum_25108"",""bidfloorcur"":""USD"",""ext"":{},""secure"":0,""instl"":0}],""bcat"":[""IAB25-3"",""BSW1"",""BSW2"",""BSW10"",""BSW4"",""IAB26""],""regs"":{""ext"":{""gdpr"":1}},""ext"":{""wt"":1,""clktrkrq"":0,""is_secure"":0,""ssp"":""gumgum""},""at"":2}";

            var bidStream = new MemoryStream(Encoding.UTF8.GetBytes(rawBid));
            var request = await context.ReadFrom<BidRequest>(bidStream, false, SerializationFormat.JSON);

            Assert.IsNotNull(request, "request should be deserialized!");
        }
    }
}