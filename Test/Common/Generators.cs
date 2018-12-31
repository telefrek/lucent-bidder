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
                Creatives = new List<Creative>()
                {
                    CreativeGenerator.GenerateCreative(),
                }
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
                Contents = new List<CreativeContent>()
                {
                    GenerateContent(),
                }
            };
        }

        public static CreativeContent GenerateContent()
        {
            var content = new CreativeContent
            {
                CanScale = false,
                H = 100,
                W = 100,
                ContentType = ContentType.Banner,
                MimeType = "image/png",
            };
            content.HydrateFilter();

            return content;
        }
    }

    public class BidGenerator
    {
        public static BidRequest GenerateBid()
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
                        }
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