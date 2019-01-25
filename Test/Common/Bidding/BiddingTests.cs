using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Bootstrap;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Exchanges;
using Lucent.Common.Filters;
using Lucent.Common.Messaging;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Lucent.Common.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Bidding
{
    [TestClass]
    public class BiddingTests
    {
        LucentTestWebHost<OrchestrationStartup> _orchestrationHost;
        LucentTestWebHost<BidderStartup> _biddingHost;
        HttpClient _orchestrationClient;
        HttpClient _biddingClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _orchestrationHost = new LucentTestWebHost<OrchestrationStartup>();
            _biddingHost = new LucentTestWebHost<BidderStartup>();

            _orchestrationClient = _orchestrationHost.CreateClient();
            _biddingClient = _biddingHost.CreateClient();
        }



        [TestMethod]
        //[Ignore]
        public async Task TestSuccessfulBid()
        {
            await SetupBidderFilters();
            var exchangeId = SequentialGuid.NextGuid();
            var bid = BidGenerator.GenerateBid();

            var serializationContext = _biddingHost.Provider.GetRequiredService<ISerializationContext>();

            // Bid, no campaign or exchange should fail
            var resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            var campaign = await SetupCampaign();

            // Bid again, should be failed
            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // Add the exchange
            await SetupExchange(exchangeId);

            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // Add budget

            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            // Verify the response
            var bidResponse = await VerifyBidResponse(resp, serializationContext, campaign);
            Assert.IsNotNull(bidResponse);

            // Ensure we filter out an invalid bid
            bid.Impressions.First().Banner.H = 101;
            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // Ensure we filter out a global bid
            bid.Impressions.First().Banner.H = 100;
            bid.Site = new Site { Domain = "telefrek.com" };
            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
        }

        async Task<HttpResponseMessage> MakeBid(BidRequest bid, ISerializationContext serializationContext, Guid exchangeId)
        {
            using (var ms = new MemoryStream())
            {
                await serializationContext.WriteTo(bid, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var bidContent = new StreamContent(ms, 4096))
                {
                    return await _biddingClient.PostAsync("/v1/bidder?exchg={0}".FormatWith(exchangeId), bidContent);
                }
            }
        }

        async Task<BidResponse> VerifyBidResponse(HttpResponseMessage responseMessage, ISerializationContext serializationContext, Campaign campaign)
        {
            var response = await serializationContext.ReadFrom<BidResponse>(await responseMessage.Content.ReadAsStreamAsync(), false, SerializationFormat.JSON);
            Assert.IsNotNull(response, "Bid response should not be null");
            Assert.IsNotNull(response.Bids, "Bids should be present");
            var seatBid = response.Bids.First();
            Assert.IsNotNull(seatBid.Bids, "Bids must be part of seatbid");
            var campaignBid = seatBid.Bids.First();
            Assert.IsNotNull(campaignBid);
            Assert.AreEqual(campaign.Id, campaignBid.CampaignId, "Only one campaign should exist");

            return response;
        }

        async Task SetupBidderFilters()
        {
            var entity = new BidderFilter
            {
                Id = SequentialGuid.NextGuid().ToString(),
                BidFilter = new BidFilter
                {
                    SiteFilters = new[] { typeof(Site).CreateFilter(FilterType.IN, "Domain", "telefrek") }
                }
            };

            var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();

            using (var ms = new MemoryStream())
            {
                await context.WriteTo(entity, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content = new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var resp = await _orchestrationClient.PostAsync("/api/filters", content);
                    Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
                }
            }
        }

        async Task SetupExchange(Guid id)
        {
            var target = Path.Combine(Directory.GetCurrentDirectory(), "SimpleExchange.dll");
            Assert.IsTrue(File.Exists(target), "Corrupt test environment");

            var entity = new Exchange
            {
                Name = "test",
                Id = id,
                LastCodeUpdate = DateTime.UtcNow,
            };

            using (var client = _orchestrationHost.CreateClient())
            {
                var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();

                using (var ms = new MemoryStream())
                {
                    await context.WriteTo(entity, ms, true, SerializationFormat.JSON);
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var content = new StreamContent(ms, 4092))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var resp = await client.PostAsync("/api/exchanges", content);
                        Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
                    }
                }

                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(File.OpenRead(target)), "exchange", "simple.dll");

                    using (
                       var message =
                           await client.PutAsync("/api/exchanges/{0}".FormatWith(id), content))
                    {
                        Assert.AreEqual(HttpStatusCode.Accepted, message.StatusCode);
                    }
                }
            }
        }

        async Task<Campaign> SetupCampaign()
        {
            var campaign = CampaignGenerator.GenerateCampaign();
            var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();

            using (var ms = new MemoryStream())
            {
                await context.WriteTo(campaign, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content =
                    new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var resp = await _orchestrationClient.PostAsync("/api/campaigns", content);
                    Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
                }
            }

            var creative = await SetupCreative();
            campaign.CreativeIds = new String[]{creative.Id};

            using (var ms = new MemoryStream())
            {
                await context.WriteTo(campaign, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content =
                    new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var resp = await _orchestrationClient.PutAsync("/api/campaigns/{0}".FormatWith(campaign.Id), content);
                    Assert.AreEqual(HttpStatusCode.Accepted, resp.StatusCode);
                }
            }

            return campaign;
        }


        async Task<Creative> SetupCreative()
        {
            var entity = CreativeGenerator.GenerateCreative();
            var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();

            using (var ms = new MemoryStream())
            {
                await context.WriteTo(entity, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content =
                    new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var resp = await _orchestrationClient.PostAsync("/api/creatives", content);
                    Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
                }
            }

            return entity;
        }
    }
}