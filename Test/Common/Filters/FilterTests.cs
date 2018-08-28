using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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

        class Filter<T>
        {
            public string Property { get; set; }
            public object Value { get; set; }
            public object[] Values { get; set; }
            public FilterType FilterType { get; set; }
        }

        Func<T, bool> CreateFilter<T>(Filter<T> filter)
        {
            var fType = filter.GetType().GetGenericArguments()[0];
            var param1 = Expression.Parameter(fType, "p1");
            var prop1 = Expression.Property(param1, filter.Property);

            Expression exp = null;
            switch (filter.FilterType)
            {
                case FilterType.NEQ:
                    exp = Expression.NotEqual(prop1, Expression.Constant(filter.Value));
                    break;
                default:
                    exp = Expression.Equal(prop1, Expression.Constant(filter.Value));
                    break;
            }

            var ftype = typeof(Func<,>).MakeGenericType(fType, typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { exp, new ParameterExpression[] { param1 } });
            return (Func<T, bool>)comp.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(comp, new object[] { });
        }

        enum FilterType
        {
            EQ = 0,
            NEQ = 1,
            GT = 2,
            GTE = 3,
            LT = 4,
            LTE = 5,
            IN = 6,
            NOTIN = 7,
        }

        MethodInfo makeLambda = typeof(Expression).GetMethods().Where(m =>
                m.Name == "Lambda" && m.IsGenericMethod && m.GetGenericArguments().Length == 1
                ).First();

        class FilterSerializer<T> : IEntitySerializer<Filter<T>>
        {
            public Filter<T> Read(ISerializationStreamReader serializationStreamReader) => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

            public async Task<Filter<T>> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
            {
                if (serializationStreamReader.Token == SerializationToken.Unknown)
                    if (!await serializationStreamReader.HasNextAsync())
                        return null;

                var filter = new Filter<T>();

                while (await serializationStreamReader.HasMorePropertiesAsync())
                {
                    var propId = serializationStreamReader.Id;
                    switch (propId.Name)
                    {
                        case "":
                            switch (propId.Id)
                            {
                                case 0:
                                    filter.Property = await serializationStreamReader.ReadStringAsync();
                                    break;
                                case 1:
                                    filter.FilterType = (FilterType)(await serializationStreamReader.ReadIntAsync());
                                    break;
                                case 2:
                                    filter.Value = await serializationStreamReader.ReadStringAsync();
                                    break;
                                default:
                                    await serializationStreamReader.SkipAsync();
                                    break;
                            }
                            break;
                        case "property":
                            filter.Property = await serializationStreamReader.ReadStringAsync();
                            break;
                        case "type":
                            filter.FilterType = (FilterType)(await serializationStreamReader.ReadIntAsync());
                            break;
                        case "value":
                            filter.Value = await serializationStreamReader.ReadStringAsync();
                            break;
                        default:
                            await serializationStreamReader.SkipAsync();
                            break;
                    }
                }

                // Get the propert
                var prop = typeof(T).GetProperty(filter.Property ?? "", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.Public);

                // This is a partial hack that will have some edge cases for sure
                if (prop != null && filter.Value != null)
                {
                    if (prop.PropertyType.IsEnum)
                        filter.Value = Enum.Parse(prop.PropertyType, filter.Value as string);
                    else
                        filter.Value = Convert.ChangeType(filter.Value, prop.PropertyType);
                }

                return filter;
            }

            public void Write(ISerializationStreamWriter serializationStreamWriter, Filter<T> instance) => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

            public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Filter<T> instance, CancellationToken token)
            {
                if (instance != null)
                {
                    serializationStreamWriter.StartObject();
                    await serializationStreamWriter.WriteAsync(new PropertyId { Id = 0, Name = "property" }, instance.Property);
                    await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "type" }, (int)instance.FilterType);
                    await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "value" }, instance.Value.ToString());
                    serializationStreamWriter.EndObject();
                    await serializationStreamWriter.FlushAsync();
                }
            }
        }

        [TestMethod]
        public void FilterSandbox()
        {
            var testGeo = new Geo { Country = "USA", Region = "East Coast", City = "Seattle", GeoType = GeoType.GPS, ISP = ISP.ip2location, UtcOffset = -7 };
            var testUser = new User { Geo = testGeo, Gender = Gender.Female, YOB = 1985 };
            var testDevice = new Device { Geography = testGeo, DeviceType = DeviceType.Phone, OS = "ios" };

            var countryFilter = new Filter<Geo> { Property = "Country", Value = "USA" };
            var utcFilter = new Filter<Geo> { Property = "UtcOffset", Value = -7 };
            var ispFilter = new Filter<Geo> { Property = "ISP", Value = ISP.MaxMind };
            var genderFilter = new Filter<User> { Property = "Gender", Value = Gender.Male };
            var deviceOsFilter = new Filter<Device> { Property = "OS", Value = "ios" };
            var deviceTypeFilter = new Filter<Device> { Property = "DeviceType", Value = DeviceType.Phone };

            Assert.IsTrue(CreateFilter(countryFilter).Invoke(testGeo));
            Assert.IsTrue(CreateFilter(utcFilter).Invoke(testGeo));
            Assert.IsFalse(CreateFilter(ispFilter).Invoke(testGeo));

            Assert.IsTrue(CreateFilter(utcFilter).Invoke(testUser.Geo));
            Assert.IsFalse(CreateFilter(ispFilter).Invoke(testDevice.Geography));

            Assert.IsFalse(CreateFilter(genderFilter).Invoke(testUser));
            Assert.IsTrue(CreateFilter(deviceOsFilter).Invoke(testDevice));
            Assert.IsTrue(CreateFilter(deviceTypeFilter).Invoke(testDevice));

            var geoSerializer = new FilterSerializer<Geo>();
            var userSerializer = new FilterSerializer<User>();
            var deviceSerializer = new FilterSerializer<Device>();

            foreach (var format in new[] { SerializationFormat.JSON, SerializationFormat.PROTOBUF })
            {
                using (var ms = new MemoryStream())
                {
                    geoSerializer.Write(ms.WrapSerializer(ServiceProvider, format, true).Writer, countryFilter);
                    ms.Seek(0, SeekOrigin.Begin);

                    var countryCopy = geoSerializer.Read(ms.WrapSerializer(ServiceProvider, format, false).Reader);

                    Assert.IsNotNull(countryCopy, "Null countryCopy");
                    Assert.IsTrue(CreateFilter(countryCopy).Invoke(testGeo));
                }

                using (var ms = new MemoryStream())
                {
                    geoSerializer.Write(ms.WrapSerializer(ServiceProvider, format, true).Writer, utcFilter);
                    ms.Seek(0, SeekOrigin.Begin);

                    var utcFilterCopy = geoSerializer.Read(ms.WrapSerializer(ServiceProvider, format, false).Reader);

                    Assert.IsNotNull(utcFilterCopy, "Null utc");
                    Assert.IsTrue(CreateFilter(utcFilterCopy).Invoke(testGeo));
                    Assert.IsTrue(CreateFilter(utcFilterCopy).Invoke(testUser.Geo));
                    Assert.IsTrue(CreateFilter(utcFilterCopy).Invoke(testDevice.Geography));
                }

                using (var ms = new MemoryStream())
                {
                    userSerializer.Write(ms.WrapSerializer(ServiceProvider, format, true).Writer, genderFilter);
                    ms.Seek(0, SeekOrigin.Begin);
                    var contents = Encoding.UTF8.GetString(ms.ToArray());

                    var genderCopy = userSerializer.Read(ms.WrapSerializer(ServiceProvider, format, false).Reader);

                    Assert.IsNotNull(genderCopy, "Null gender");
                    Assert.IsFalse(CreateFilter(genderCopy).Invoke(testUser));
                }

                using (var ms = new MemoryStream())
                {
                    deviceSerializer.Write(ms.WrapSerializer(ServiceProvider, format, true).Writer, deviceTypeFilter);
                    ms.Seek(0, SeekOrigin.Begin);

                    var deviceTypeCopy = deviceSerializer.Read(ms.WrapSerializer(ServiceProvider, format, false).Reader);

                    Assert.IsNotNull(deviceTypeCopy, "Null devicetype");
                    Assert.IsTrue(CreateFilter(deviceTypeCopy).Invoke(testDevice));
                }
            }
        }
    }
}