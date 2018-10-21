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
            services.AddLucentServices(Configuration, localOnly:true);
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
                ImpressionFilters = new[] { new Filter{ Property = "BidCurrency", Value = "CAN" } },
                UserFilters = new[] { new Filter { Property = "Gender", Value = Gender.Unknown } },
                GeoFilters = new[] { new Filter { Property = "Country", Value = "CAN" } }
            };


            var f = bFilter.GenerateCode();

            Assert.IsFalse(f.Invoke(req), "Filter should not have matched");

            req.User.Geo = new Geo { Country = "CAN" };

            Assert.IsTrue(f.Invoke(req), "Filter should have matched");
        }
    }
}