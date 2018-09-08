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
using Lucent.Common.Filters;
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

        bool BaseLine(BidRequest bid)
        {
            if (bid.Impressions != null)
            {
                foreach (var imp in bid.Impressions)
                {
                    if (imp.BidCurrency == "CAN")
                        return true;
                }
            }

            if (bid.User != null && (bid.User.Gender == Gender.Unknown ||
                (bid.User.Geo != null && bid.User.Geo.Country == "CAN")))
                return true;

            return false;
        }


        [TestMethod]
        public void TestBidFilter()
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test" } },
                User = new User { Gender = Gender.Male }
            };

            var bFilter = new BidFilter
            {
                ImpressionFilters = new[] { new Filter<Impression> { Property = "BidCurrency", Value = "CAN" } },
                UserFilters = new[] { new Filter<User> { Property = "Gender", Value = Gender.Unknown } },
                GeoFilters = new[] { new Filter<Geo> { Property = "Country", Value = "CAN" } }
            };


            var f = bFilter.GenerateCode();

            Assert.IsFalse(f.Invoke(req), "Filter should not have matched");

            req.User.Geo = new Geo { Country = "CAN" };

            Assert.IsTrue(f.Invoke(req), "Filter should have matched");
        }

        [TestMethod]
        public void FilterSandbox()
        {
            var testGeo = new Geo { Country = "USA", Region = "East Coast", City = "Seattle", GeoType = GeoType.GPS, ISP = ISP.ip2location, UtcOffset = -7 };
            var testUser = new User { Geo = testGeo, Gender = Gender.Female, YOB = 1985 };
            var testDevice = new Device { Geo = testGeo, DeviceType = DeviceType.Phone, OS = "ios" };

            var countryFilter = new Filter<Geo> { Property = "Country", Value = "USA" };
            var utcFilter = new Filter<Geo> { Property = "UtcOffset", Value = -7 };
            var ispFilter = new Filter<Geo> { Property = "ISP", Value = ISP.MaxMind };
            var genderFilter = new Filter<User> { Property = "Gender", Value = Gender.Male };
            var deviceOsFilter = new Filter<Device> { Property = "OS", Value = "ios" };
            var deviceTypeFilter = new Filter<Device> { Property = "DeviceType", Value = DeviceType.Phone };


            var geoFilter = new[] { countryFilter, utcFilter, ispFilter }.CreateFilter();

            Assert.IsTrue(new[] { countryFilter, utcFilter }.CreateFilter().Invoke(testGeo), "Filter should have passed");
            Assert.IsFalse(new[] { ispFilter }.CreateFilter().Invoke(testGeo), "ISP filter should have failed");

            testGeo.ISP = ISP.MaxMind;
            Assert.IsTrue(geoFilter.Invoke(testGeo), "ISP filter should have passed");
        }
    }
}