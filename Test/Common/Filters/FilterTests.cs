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
            services.AddLucentServices(Configuration, localOnly: true);
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

            if (bid.User != null && (bid.User.Gender == "U" ||
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
                User = new User { Gender = "M", Geo = new Geo { Country = "CAN" } }
            };

            var bFilter = new BidFilter
            {
                ImpressionFilters = new[] { new Filter { Property = "BidCurrency", Value = "CAN" } },
                UserFilters = new[] { new Filter { Property = "Gender", Value = "U" } },
                GeoFilters = new[] { new Filter { FilterType = FilterType.IN, Property = "Country", Value = "usa" } }
            };


            var f = bFilter.GenerateFilter();

            Assert.IsFalse(f.Invoke(req), "Filter should not have matched");

            req.User.Geo = new Geo { Country = "USA" };

            Assert.IsTrue(f.Invoke(req), "Filter should have matched");
        }

        [TestMethod]
        public void TestTargets()
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test", } },
                User = new User { Gender = "M" },
            };

            var bFilter = new BidTargets
            {
                ImpressionTargets = new[] { new Target { Property = "BidCurrency", Value = "USD", Modifier = 1 }, new Target { Property = "Banner", TargetType = FilterType.HASVALUE, Modifier = .5 } },
                GeoTargets = new[] { new Target { TargetType = FilterType.EQ, Property = "Country", Value = "CAN", Modifier = 1.1d } },
                UserTargets = new[]{new Target { TargetType = FilterType.EQ, Property = "Gender", Value="M", Modifier = 0.2},
                new Target { TargetType = FilterType.EQ, Property = "Gender", Value="F"}}
            };

            var f = bFilter.GenerateTargets(1.5);

            Assert.AreEqual(1.5 * .2, f.Invoke(req), 0.001d, "No values set");

            req.User.Geo = new Geo { Country = "CAN" };

            Assert.AreEqual(1.5 * .2 * 1.1, f.Invoke(req), 0.001d, "Value should have been positive");

            req.Impressions.First().Banner = new Banner();

            Assert.AreEqual(1.5 * .2 * 1.1 * .5, f.Invoke(req), 0.001d, "Value should have been negative");

            req.User.Gender = "F";
            Assert.AreEqual(1.5 * 1.1 * .5, f.Invoke(req), 0.001d, "Value should have been boosted by gender");
        }

        [TestMethod]
        public void TestDoubleFilters()
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test" } },
                User = new User { Gender = "M" },
            };

            var bFilter = new BidFilter
            {
                ImpressionFilters = new[] { new Filter { Property = "BidCurrency", Value = "CAN" }, new Filter { Property = "Banner", FilterType = FilterType.HASVALUE } },
                UserFilters = new[] { new Filter { Property = "Gender", Value = "U" } },
                GeoFilters = new[] { new Filter { FilterType = FilterType.EQ, Property = "Country", Value = "CAN" } }
            };

            var f = bFilter.GenerateFilter();

            Assert.IsFalse(f.Invoke(req), "Filter should not have matched");

            req.User.Geo = new Geo { Country = "CAN" };

            Assert.IsTrue(f.Invoke(req), "Filter should have matched");
        }


        [TestMethod]
        public void TestHasValueFilter()
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test" } },
                User = new User { Gender = "M" }
            };

            var bFilter = new BidFilter
            {
                ImpressionFilters = new[] { new Filter { Property = "BidCurrency", Value = "CAN" } },
                UserFilters = new[] { new Filter { Property = "Gender", Value = "U" } },
                GeoFilters = new[] { new Filter { FilterType = FilterType.HASVALUE, Property = "Country", Value = "CAN" } }
            };


            var f = bFilter.GenerateFilter();

            Assert.IsFalse(f.Invoke(req), "Filter should not have matched");

            req.User.Geo = new Geo { Country = "CAN" };

            Assert.IsTrue(f.Invoke(req), "Filter should have matched");
        }


        [TestMethod]
        public void TestInFilter()
        {
            var req = new BidRequest
            {
                Impressions = new Impression[] { new Impression { ImpressionId = "test" } },
                User = new User { Gender = "M" },
                Site = new Site { SiteCategories = new string[] { "BCAT1" }, Domain = "lucentbid.com" }
            };

            var bFilter = new BidFilter
            {
                SiteFilters = new[] { new Filter { FilterType = FilterType.IN, Property = "SiteCategories", Values = new FilterValue[] { "BCAT1" } },
                    new Filter { FilterType = FilterType.IN, Property = "SiteCategories", Value = "BCAT3" }, new Filter { FilterType = FilterType.IN, Property = "Domain", Values = new FilterValue[]{"telefrek.com", "telefrek.co", "bad" }} },
            };

            var f = bFilter.GenerateFilter();

            Assert.IsTrue(f.Invoke(req), "Bid should have been filtered");

            req.Site.SiteCategories = new string[] { "BCAT2" };
            Assert.IsFalse(f.Invoke(req), "Bid should not have been filtered");

            req.Site.SiteCategories = new string[] { "BCAT2", "BCAT3" };
            Assert.IsTrue(f.Invoke(req), "Bid should have been filtered");


            req.Site.SiteCategories = new string[] { "BCAT2" };
            req.Site.Domain = "telefrek.co";
            Assert.IsTrue(f.Invoke(req), "Bid should have been filtered");

            req.Site.Domain = "adobada";
            Assert.IsTrue(f.Invoke(req), "Bid should have been filtered");

            req.Site.Domain = null;
            Assert.IsFalse(f.Invoke(req), "Bid should not have been filtered");

            req.Site.SiteCategories = new string[] { "BCAT1" };
            Assert.IsTrue(f.Invoke(req), "Bid should have been filtered");
        }
    }
}