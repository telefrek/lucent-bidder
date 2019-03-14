using System;
using System.Collections.Generic;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Test
{
    public class CampaignGenerator
    {
        public static Campaign GenerateCampaign()
        {
            return new Campaign
            {
                Id = SequentialGuid.NextGuid().ToString(),
                Name = "Test Campaign",
                LandingPage = "https://www.lucentbid.com",
                AdDomains = new string[] { "lucentbid.com", "lucentbid.co" },
                BuyerId = "buyerid",
                BundleId = "bundle_1",
                BidFilter = new BidFilter
                {
                    GeoFilters = new Filters.Filter[]
                    {
                        typeof(Geo).CreateFilter(FilterType.IN, "Country", "USA"),
                    }
                },
                ConversionPrice = 1,
            };
        }
    }

    public class CreativeGenerator
    {
        public static Creative GenerateCreative()
        {
            return new Creative
            {
                Id = "crid1",
            };
        }
    }

    public class BidGenerator
    {
        static Random _rng = new Random();
        public static BidRequest GenerateBid(double cpmMax = 2)
        {
            return new BidRequest
            {
                Id = SequentialGuid.NextGuid().ToString(),
                Impressions = new Impression[]
                {
                    new Impression
                    {
                        ImpressionId = "1",
                        Banner = new Banner
                        {
                            H = 100,
                            W = 100,
                            MimeTypes = new string[]{"image/png"}
                        },
                        BidFloor = _rng.NextDouble() * cpmMax,
                    }
                },
                User = new User
                {
                    Geo = new Geo
                    {
                        Country = "USA",
                    }
                }
            };
        }
    }
}