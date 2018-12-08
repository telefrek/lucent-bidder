using System;
using System.Collections.Generic;
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

        [TestMethod]
        public async Task TestNewSerializer()
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

            using (var ms = new MemoryStream())
            {
                using (var protoWriter = new LucentJsonWriter(ms, true))
                    await TestWrite(protoWriter, geo);

                ms.Seek(0, SeekOrigin.Begin);

                var protoReader = new LucentJsonReader(ms, false);
                var testGeo = await TestRead(await protoReader.GetObjectReader());

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                using (var protoWriter = new LucentProtoWriter(ms, true))
                    await TestWrite(protoWriter, geo);
                ms.Seek(0, SeekOrigin.Begin);

                var protoReader = new LucentProtoReader(ms, false);
                var testGeo = await TestRead(protoReader);

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }
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

            var serializationContext = ServiceProvider.GetRequiredService<ISerializationContext>();

            using (var ms = new MemoryStream())
            {
                using (var jsonWriter = new LucentJsonWriter(ms, true))
                    await TestWrite(jsonWriter, geo);

                ms.Seek(0, SeekOrigin.Begin);

                var jsonReader = new LucentJsonReader(ms, false);
                var testGeo = await TestReadOne<Geo>(await jsonReader.GetObjectReader());

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                using (var protoWriter = new LucentProtoWriter(ms, true))
                    await TestWrite(protoWriter, geo);
                ms.Seek(0, SeekOrigin.Begin);

                var protoReader = new LucentProtoReader(ms, false);
                var testGeo = await TestReadOne<Geo>(protoReader);

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                using (var jsonWriter = new LucentJsonWriter(ms, true))
                    await TestWriteOne(jsonWriter, geo, serializationContext);

                ms.Seek(0, SeekOrigin.Begin);

                var jsonReader = new LucentJsonReader(ms, false);
                var testGeo = await TestRead(await jsonReader.GetObjectReader());

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                using (var protoWriter = new LucentProtoWriter(ms, true))
                    await TestWriteOne(protoWriter, geo, serializationContext);
                ms.Seek(0, SeekOrigin.Begin);

                var protoReader = new LucentProtoReader(ms, false);
                var testGeo = await TestRead(protoReader);

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                using (var jsonWriter = new LucentJsonWriter(ms, true))
                    await TestWriteOne(jsonWriter, geo, serializationContext);

                ms.Seek(0, SeekOrigin.Begin);

                var jsonReader = new LucentJsonReader(ms, false);
                var testGeo = await TestReadOne<Geo>(await jsonReader.GetObjectReader());

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }

            using (var ms = new MemoryStream())
            {
                using (var protoWriter = new LucentProtoWriter(ms, true))
                    await TestWriteOne(protoWriter, geo, serializationContext);
                ms.Seek(0, SeekOrigin.Begin);

                var protoReader = new LucentProtoReader(ms, false);
                var testGeo = await TestReadOne<Geo>(protoReader);

                var tmp = JsonConvert.SerializeObject(geo);
                Assert.AreEqual(tmp, JsonConvert.SerializeObject(testGeo));
            }
        }

        async Task<T> TestReadOne<T>(ILucentReader reader) where T : new()
        {
            PropertyId prop = await reader.NextAsync();
            Assert.IsTrue(prop.Id == 1 || prop.Name == "geo");
            using (var objReader = await reader.GetObjectReader())
                return await CreateReader<T>(objReader);
        }

        async Task TestWriteOne<T>(ILucentWriter writer, T instance, ISerializationContext context) where T : new()
        {
            using (var objWriter = await writer.CreateObjectWriter(new PropertyId { Id = 1, Name = "geo" }))
            {
                await context.Write(objWriter, instance);
                await objWriter.EndObject();
            }

            await writer.Flush();
        }

        Task MapProperty<T>(PropertyId propertyId, ILucentObjectReader reader, T instance)
        {
            var props = from p in typeof(T).GetProperties()
                        let attr = p.GetCustomAttributes(typeof(SerializationPropertyAttribute), true)
                        where attr.Length == 1
                        select new { Property = p, Attribute = attr.Single() as SerializationPropertyAttribute };

            var def = props.FirstOrDefault(p => p.Attribute.Name.Equals(propertyId.Name) || p.Attribute.Id.Equals(propertyId.Id));

            if (def != null)
            {
                if (def.Property.PropertyType.IsEnum)
                    return reader.NextInt().ContinueWith((t, g) => def.Property.SetValue(g, Enum.ToObject(def.Property.PropertyType, t.Result)), instance);
                else if (def.Property.PropertyType.IsPrimitive)
                {
                    if (def.Property.PropertyType == typeof(int))
                        return reader.NextInt().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                    else if (def.Property.PropertyType == typeof(uint))
                        return reader.NextUInt().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                    else if (def.Property.PropertyType == typeof(long))
                        return reader.NextLong().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                    else if (def.Property.PropertyType == typeof(ulong))
                        return reader.NextULong().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                    else if (def.Property.PropertyType == typeof(double))
                        return reader.NextDouble().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                    else if (def.Property.PropertyType == typeof(float))
                        return reader.NextSingle().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                    else if (def.Property.PropertyType == typeof(bool))
                        return reader.NextBoolean().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                }
                else if (def.Property.PropertyType == typeof(string))
                    return reader.NextString().ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                else if (def.Property.PropertyType.IsArray) // let's read an array!
                {
                    ((Task<object[]>)this.GetType().GetMethods().Single(m => m.Name == "CreateArrayReader").MakeGenericMethod(def.Property.PropertyType.GetElementType()).Invoke(this, new object[] { reader.GetArrayReader() })).ContinueWith((t, g) => def.Property.SetValue(g, t.Result), instance);
                }
            }

            return reader.Skip();
        }

        Task<T> CreateReader<T>(ILucentObjectReader reader) where T : new()
        {
            var asm = AsyncTaskMethodBuilder<T>.Create();
            var rsm = new ReaderStateMachine<T>
            {
                ResultBuilder = asm,
                Result = new T(),
                State = -1,
                Reader = reader,
                Map = MapProperty,
            };

            asm.Start(ref rsm);

            return rsm.ResultBuilder.Task;
        }

        Task<T[]> CreateArrayReader<T>(ILucentArrayReader reader)
        {
            var asm = AsyncTaskMethodBuilder<T[]>.Create();
            var rsm = new ArrayReaderStateMachine<T>
            {
                ResultBuilder = asm,
                Results = new List<T>(),
                State = -1,
                Reader = reader,
                ReaderType = typeof(T)
            };

            asm.Start(ref rsm);

            return rsm.ResultBuilder.Task;

        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        struct ArrayReaderStateMachine<T> : IAsyncStateMachine
        {
            public ILucentArrayReader Reader;
            public int State;
            public AsyncTaskMethodBuilder<T[]> ResultBuilder;
            public List<T> Results;
            public Type ReaderType;
            private TaskAwaiter<T[]> _resAwaiter;
            private TaskAwaiter<bool> _checkAwaiter;
            private TaskAwaiter _readAwaiter;

            public void MoveNext()
            {
                try
                {
                    switch (State)
                    {
                        case -1:
                            _checkAwaiter = Reader.IsComplete().GetAwaiter();
                            if (!_checkAwaiter.IsCompleted)
                            {
                                State = 0;
                                ResultBuilder.AwaitUnsafeOnCompleted(ref _checkAwaiter, ref this);
                                return;
                            }
                            goto case 0;
                        case 0:
                            if (!_checkAwaiter.GetResult())
                            {
                                // do parameter assignment
                                if (ReaderType.IsEnum)
                                    _readAwaiter = Reader.NextInt().ContinueWith((t, res) => ((List<T>)res).Add((T)Enum.ToObject(typeof(T), t.Result)), Results).GetAwaiter();
                                else if (ReaderType.IsPrimitive)
                                {
                                    if (ReaderType == typeof(int))
                                        _readAwaiter = Reader.NextInt().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(uint))
                                        _readAwaiter = Reader.NextUInt().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(long))
                                        _readAwaiter = Reader.NextLong().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(ulong))
                                        _readAwaiter = Reader.NextULong().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(double))
                                        _readAwaiter = Reader.NextDouble().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(string))
                                        _readAwaiter = Reader.NextString().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(float))
                                        _readAwaiter = Reader.NextSingle().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else if (ReaderType == typeof(bool))
                                        _readAwaiter = Reader.NextBoolean().ContinueWith((t, res) => ((List<T>)res).Add((T)(object)t.Result), Results).GetAwaiter();
                                    else
                                        _readAwaiter = Reader.Skip().GetAwaiter();
                                }
                                else
                                    _readAwaiter = Reader.Skip().GetAwaiter();

                                if (!_readAwaiter.IsCompleted)
                                {
                                    State = -1;
                                    ResultBuilder.AwaitUnsafeOnCompleted(ref _readAwaiter, ref this);
                                    return;
                                }

                                goto case -1;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    State = -2;
                    using (Reader)
                        ResultBuilder.SetException(e);
                    return;
                }

                State = -2;
                using (Reader)
                    ResultBuilder.SetResult(Results.ToArray());
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine) => ResultBuilder.SetStateMachine(stateMachine);
        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        struct ReaderStateMachine<T> : IAsyncStateMachine
        {
            public ILucentObjectReader Reader;
            public int State;
            public AsyncTaskMethodBuilder<T> ResultBuilder;
            public T Result;

            public Func<PropertyId, ILucentObjectReader, T, Task> Map;
            private TaskAwaiter<PropertyId> _propAwaiter;
            private TaskAwaiter<T> _resAwaiter;
            private TaskAwaiter<bool> _checkAwaiter;
            private TaskAwaiter _readAwaiter;

            public void MoveNext()
            {
                try
                {
                    switch (State)
                    {
                        case -1:
                            _checkAwaiter = Reader.IsComplete().GetAwaiter();
                            if (!_checkAwaiter.IsCompleted)
                            {
                                State = 0;
                                ResultBuilder.AwaitUnsafeOnCompleted(ref _checkAwaiter, ref this);
                                return;
                            }
                            goto case 0;
                        case 0:
                            if (!_checkAwaiter.GetResult())
                            {
                                // do parameter assignment
                                _propAwaiter = Reader.NextAsync().GetAwaiter();
                                if (!_propAwaiter.IsCompleted)
                                {
                                    State = 1;
                                    ResultBuilder.AwaitUnsafeOnCompleted(ref _propAwaiter, ref this);
                                    return;
                                }

                                goto case 1;
                            }
                            break;
                        case 1:
                            var propId = _propAwaiter.GetResult();
                            if (propId != null)
                            {
                                // Need to map the property to a property setter
                                var aw = Map.Invoke(propId, Reader, Result).GetAwaiter();
                                if (!aw.IsCompleted)
                                {
                                    State = -1;
                                    ResultBuilder.AwaitUnsafeOnCompleted(ref aw, ref this);
                                    return;
                                }
                                State = -1;
                                goto case -1;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    State = -2;
                    ResultBuilder.SetException(e);
                    return;
                }

                State = -2;
                ResultBuilder.SetResult(Result);
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine) => ResultBuilder.SetStateMachine(stateMachine);
        }        

        async Task TestWrite(ILucentWriter writer, Geo geo)
        {
            using (var objWriter = await writer.CreateObjectWriter(new PropertyId { Id = 1, Name = "geo" }))
            {
                await objWriter.WriteAsync(new PropertyId { Id = 1, Name = "lat" }, geo.Latitude);
                await objWriter.WriteAsync(new PropertyId { Id = 2, Name = "lon" }, geo.Longitude);
                await objWriter.WriteAsync(new PropertyId { Id = 3, Name = "country" }, geo.Country);
                await objWriter.WriteAsync(new PropertyId { Id = 4, Name = "region" }, geo.Region);
                await objWriter.WriteAsync(new PropertyId { Id = 5, Name = "regionfips104" }, geo.RegionFips);
                await objWriter.WriteAsync(new PropertyId { Id = 6, Name = "metro" }, geo.Metro);
                await objWriter.WriteAsync(new PropertyId { Id = 7, Name = "city" }, geo.City);
                await objWriter.WriteAsync(new PropertyId { Id = 8, Name = "zip" }, geo.Zip);
                await objWriter.WriteAsync(new PropertyId { Id = 9, Name = "type" }, (int)geo.GeoType);
                await objWriter.WriteAsync(new PropertyId { Id = 10, Name = "utcoffset" }, geo.UtcOffset);
                await objWriter.WriteAsync(new PropertyId { Id = 11, Name = "accuracy" }, geo.Accuracy);
                await objWriter.WriteAsync(new PropertyId { Id = 12, Name = "lastfix" }, geo.LastFixed);
                await objWriter.WriteAsync(new PropertyId { Id = 13, Name = "ipservice" }, (int)geo.ISP);

                await objWriter.EndObject();
                await objWriter.Flush();
            }

            await writer.Flush();
        }

        async Task<Geo> TestRead(ILucentReader reader)
        {
            Geo geo = null;
            PropertyId prop = await reader.NextAsync();
            Assert.IsTrue(prop.Id == 1 || prop.Name == "geo");
            using (var objReader = await reader.GetObjectReader())
            {
                geo = new Geo();
                while (!await objReader.IsComplete() && (prop = await objReader.NextAsync()) != null)
                {

                    if (prop.Id == 1 || prop.Name == "lat")
                        geo.Latitude = await objReader.NextDouble();
                    else if (prop.Id == 2 || prop.Name == "lon")
                        geo.Longitude = await objReader.NextDouble();
                    else if (prop.Id == 3 || prop.Name == "country")
                        geo.Country = await objReader.NextString();
                    else if (prop.Id == 4 || prop.Name == "region")
                        geo.Region = await objReader.NextString();
                    else if (prop.Id == 5 || prop.Name == "regionfips104")
                        geo.RegionFips = await objReader.NextString();
                    else if (prop.Id == 6 || prop.Name == "metro")
                        geo.Metro = await objReader.NextString();
                    else if (prop.Id == 7 || prop.Name == "city")
                        geo.City = await objReader.NextString();
                    else if (prop.Id == 8 || prop.Name == "zip")
                        geo.Zip = await objReader.NextString();
                    else if (prop.Id == 9 || prop.Name == "type")
                        geo.GeoType = (GeoType)(await objReader.NextInt());
                    else if (prop.Id == 10 || prop.Name == "utcoffset")
                        geo.UtcOffset = await objReader.NextInt();
                    else if (prop.Id == 11 || prop.Name == "accuracy")
                        geo.Accuracy = await objReader.NextInt();
                    else if (prop.Id == 12 || prop.Name == "lastfix")
                        geo.LastFixed = await objReader.NextInt();
                    else if (prop.Id == 13 || prop.Name == "ipservice")
                        geo.ISP = (ISP)(await objReader.NextInt());
                    else
                        await objReader.Skip();
                }
            }

            return geo;
        }

        [TestMethod]
        public async Task TestDeserializeBid()
        {
            var context = ServiceProvider.GetService<ISerializationContext>();
            var rawBid = @"{""id"":""c6987c2b-edb4-4b7b-b8cf-157af1d485e3"",""site"":{""id"":""gumgum_www.answers.com_ed2265d8"",""ref"":""http://ad32.answers.com/click.php?source=fb&param4=fb-us-de-red&param3=www.answers.com%2Farticle%2F31029589%2Finsanely-useful-life-hacks-to-make-everything-easier&param1=tattoo&param2=67660042&param5=10153631993521186&param6=6049542139960&adt=4342"",""publisher"":{""name"":""www.answers.com"",""id"":""gumgum_946353442_12535""},""name"":""www.answers.com"",""cat"":[""IAB24""],""domain"":""answers.com"",""ext"":{},""page"":""http://www.answers.com/article/31029589/insanely-useful-life-hacks-to-make-everything-easier?paramt=null&param4=fb-us-de-red&param1=tattoo&param2=67660042&s=8""},""wseat"":[""165"",""16""],""source"":{""fd"":0},""user"":{""id"":""5e29eb00-c30a-416e-9d2a-2e18901f0916"",""ext"":{""cookie_age"":64,""consent"":""Y29uc2VudCBkYXRh""},""buyeruid"":""CAESEHL-9O4oJOAiC1Y0O2EHTcE""},""device"":{""pxratio"":0,""language"":""en"",""mccmnc"":""310-005"",""w"":1920,""geo"":{""country"":""US"",""lon"":-80.237,""city"":""West Palm Beach"",""lat"":26.638,""zip"":""33414"",""region"":""FL"",""type"":2},""os"":""Windows"",""devicetype"":2,""h"":1080,""ip"":""73.139.39.18"",""js"":1,""ua"":""Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0"",""dnt"":0},""tmax"":75,""cur"":[""USD""],""imp"":[{""bidfloor"":3.213,""metric"":[{""type"":""viewability"",""value"":0.85}],""id"":""1"",""banner"":{""pos"":1,""h"":600,""battr"":[1,3,5,6,8,9,10,14,15,16],""w"":160,""format"":[{""h"":300,""w"":300},{""h"":350,""w"":300}],""btype"":[1]},""exp"":300,""tagid"":""gumgum_25108"",""bidfloorcur"":""USD"",""ext"":{},""secure"":0,""instl"":0}],""bcat"":[""IAB25-3"",""BSW1"",""BSW2"",""BSW10"",""BSW4"",""IAB26""],""regs"":{""ext"":{""gdpr"":1}},""ext"":{""wt"":1,""clktrkrq"":0,""is_secure"":0,""ssp"":""gumgum""},""at"":2}";

            var bidStream = new MemoryStream(Encoding.UTF8.GetBytes(rawBid));

            using (var wrapped = context.WrapStream(bidStream, false, SerializationFormat.JSON).Reader)
            {
                Assert.IsTrue(await wrapped.HasNextAsync());
                var bid = await wrapped.ReadAsAsync<BidRequest>();
                Assert.IsNotNull(bid);
            }
        }

        [TestMethod]
        public async Task TestProtoSerializer()
        {
            var dt = DateTime.Now;
            var context = ServiceProvider.GetService<ISerializationContext>();

            using (var ms = new MemoryStream())
            {
                using (var writer = context.CreateWriter(ms, true, SerializationFormat.PROTOBUF))
                {
                    await writer.WriteAsync(dt);
                    await writer.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = context.CreateReader(ms, true, SerializationFormat.PROTOBUF))
                {
                    var v1 = reader.ReadDateTime();
                    Assert.AreEqual(dt, v1, "date mismatch");
                }
            }

            using (var ms = new MemoryStream())
            {
                using (var writer = context.CreateWriter(ms, true, SerializationFormat.JSON))
                {
                    await writer.WriteAsync(dt);
                    await writer.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = context.CreateReader(ms, true, SerializationFormat.JSON))
                {
                    var v1 = reader.ReadDateTime();
                    Assert.AreEqual(dt, v1, "date mismatch");
                }
            }
        }
    }
}