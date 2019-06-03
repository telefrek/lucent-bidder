using System;
using System.Collections.Generic;
using System.Threading;
using Lucent.Common.Budget;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Test
{
    public class CampaignGenerator
    {
        static long cid = 0;
        static Random _rng = new Random();
        public static Campaign GenerateCampaign()
        {
            
            return new Campaign
            {
                Id = SequentialGuid.NextGuid().ToString(),
                Name = "Test Campaign " + Interlocked.Add(ref cid, 1),
                LandingPage = "https://localhost/landing?lctx={CONTEXT}",
                AdDomains = new string[] { "lucentbid.com", "lucentbid.co" },
                BuyerId = "buyerid",
                BundleId = "bundle_1",
                Actions = new PostbackAction[]{
                    new PostbackAction {
                        Name = "install",
                        Payout = _rng.NextDouble() * 5,
                    }
                },
                BidFilter = new BidFilter
                {
                    GeoFilters = new Filters.Filter[]
                    {
                        typeof(Geo).CreateFilter(FilterType.IN, "Country", "USA"),
                    }
                },
                Schedule = new CampaignSchedule
                {
                    StartDate = DateTime.UtcNow.AddMinutes(-5),
                    EndDate = DateTime.UtcNow.AddDays(1),
                },
                MaxCPM = _rng.NextDouble() * 3,
                BudgetSchedule = new BudgetSchedule
                {
                    ScheduleType = _rng.NextDouble() < .4 ? ScheduleType.Even : ScheduleType.Aggressive,
                    HourlyCap = _rng.NextDouble() * 5,
                    DailyCap = _rng.NextDouble() * 40 + 20,
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
                        Country = _rng.NextDouble() < .1 ? "CAN" : "USA",
                    }
                }
            };
        }
    }
}