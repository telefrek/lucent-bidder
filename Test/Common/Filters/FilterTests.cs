using System;
using System.Collections;
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

        class BidFilter
        {
            public Filter<Geo>[] GeoFilters { get; set; }
            public Filter<Impression>[] ImpressionFilters { get; set; }
            public Filter<User>[] UserFilters { get; set; }
            public Filter<Device>[] DeviceFilters { get; set; }
            public Filter<Site>[] SiteFilters { get; set; }
            public Filter<App>[] AppFilters { get; set; }
        }
        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "e");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("lbrk");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        bool Scaffold(BidRequest bid)
        {
            if (bid.Impressions != null)
            {
                foreach (var imp in bid.Impressions)
                {
                    if (imp.BidCurrency == "USD")
                        return true;
                }
            }

            return false;
        }

        Func<BidRequest, bool> CreateBidFilter(BidFilter bidFilter)
        {
            // Need our input parameter :)
            var bidParam = Expression.Parameter(typeof(BidRequest), "bid");

            // Need a variable to track the complex filtering
            var fValue = Expression.Variable(typeof(bool), "isFiltered");

            // Keep track of all the expressions in this chain
            var expList = new List<Expression> { };
            //expList.Add(Expression.Assign(fValue, Expression.Constant(false)));

            // Need a sentinal value for breaking in loops
            var loopBreak = Expression.Label();
            var ret = Expression.Label(typeof(bool)); // We're going to return a bool

            // Process the impressions filters
            if (bidFilter.ImpressionFilters != null)
            {
                var impProp = Expression.Property(bidParam, "Impressions");
                var impType = typeof(Impression);

                var impParam = Expression.Parameter(impType, "imp");
                var impTest = CombineFilters(bidFilter.ImpressionFilters, impParam);

                var forLoop = Expression.IfThen(Expression.NotEqual(impProp, Expression.Constant(null)),
                    ForEach(impProp, impParam, Expression.IfThen(impTest, Expression.Return(ret, Expression.Constant(true)))));

                expList.Add(forLoop);
            }

            expList.Add(Expression.Label(ret, Expression.Constant(false)));

            var final = Expression.Block(expList);

            var ftype = typeof(Func<,>).MakeGenericType(typeof(BidRequest), typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { final, new ParameterExpression[] { bidParam } });
            return (Func<BidRequest, bool>)comp.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(comp, new object[] { });
        }

        class Filter<T>
        {
            public string Property { get; set; }
            public object Value { get; set; }
            public object[] Values { get; set; }
            public FilterType FilterType { get; set; }
        }

        Expression CreateExpression<T>(Filter<T> filter, Expression p)
        {
            var prop = Expression.Property(p, filter.Property);

            Expression exp = null;
            switch (filter.FilterType)
            {
                case FilterType.NEQ:
                    exp = Expression.NotEqual(prop, Expression.Constant(filter.Value));
                    break;
                default:
                    exp = Expression.Equal(prop, Expression.Constant(filter.Value));
                    break;
            }

            return exp;
        }

        Expression CombineFilters<T>(ICollection<Filter<T>> filters, Expression target)
        {
            Expression exp = null;

            foreach (var filter in filters)
            {
                var e = CreateExpression(filter, target);

                if (exp == null)
                    exp = e;
                else
                    exp = Expression.OrElse(exp, e);
            }

            return exp;
        }

        Func<T, bool> CreateFilter<T>(ICollection<Filter<T>> filters)
        {
            var fType = typeof(T);
            var p = Expression.Parameter(fType, "p1");

            Expression exp = CombineFilters(filters, p);

            var ftype = typeof(Func<,>).MakeGenericType(fType, typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { exp, new ParameterExpression[] { p } });
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
        public void TestBidFilter()
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test" } }
            };

            var bFilter = new BidFilter
            {
                ImpressionFilters = new[] { new Filter<Impression> { Property = "BidCurrency", Value = "USD" } }
            };


            var f = CreateBidFilter(bFilter);

            Assert.IsTrue(f.Invoke(req), "Filter should have matched");

            Assert.IsTrue(Scaffold(req), "Scaffolding sucks");
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


            var geoFilter = CreateFilter(new[] { countryFilter, utcFilter, ispFilter });

            Assert.IsTrue(CreateFilter(new[] { countryFilter, utcFilter }).Invoke(testGeo), "Filter should have passed");
            Assert.IsFalse(geoFilter.Invoke(testGeo), "ISP filter should have failed");

            testGeo.ISP = ISP.MaxMind;
            Assert.IsTrue(geoFilter.Invoke(testGeo), "ISP filter should have passed");
        }
    }
}