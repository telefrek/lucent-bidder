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
using Lucent.Common.Budget;
using Lucent.Common.Client;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Bidding
{
    [TestClass]
    public class BiddingTests
    {
        LucentTestWebHost<OrchestrationStartup> _orchestrationHost;
        LucentTestWebHost<BidderStartup> _biddingHost;
        static HttpClient _orchestrationClient;
        static HttpClient _biddingClient;

        Campaign campaign;
        Creative creative;
        Guid exchangeId;
        BidderFilter bidderFilter;

        public TestContext TestContext { get; set; }

        [TestCleanup]
        public async Task TestCleanup()
        {
            if (campaign != null)
                await _orchestrationClient.DeleteAsync("/api/campaigns/{0}".FormatWith(campaign.Id));

            if (creative != null)
                await _orchestrationClient.DeleteAsync("/api/creatives/{0}".FormatWith(creative.Id));

            if (exchangeId != null)
                await _orchestrationClient.DeleteAsync("/api/exchanges/{0}".FormatWith(exchangeId));

            if (bidderFilter != null)
                await _orchestrationClient.DeleteAsync("/api/filters/{0}".FormatWith(bidderFilter.Id));
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            _orchestrationHost = new LucentTestWebHost<OrchestrationStartup>();
            _biddingHost = new LucentTestWebHost<BidderStartup>()
            {
                UpdateServices = (services) =>
                {
                    services.AddTransient<IClientManager, TestClientManger>();
                }
            };

            _orchestrationClient = _orchestrationHost.CreateClient();
            _biddingClient = _biddingHost.CreateClient();

            await Task.CompletedTask;
        }

        class TestClientManger : IClientManager
        {
            public HttpClient OrchestrationClient => _orchestrationClient;
        }

        [TestMethod]
        //[Ignore]
        public async Task TestSuccessfulBid()
        {
            await SetupBidderFilters();
            exchangeId = SequentialGuid.NextGuid();
            var bid = BidGenerator.GenerateBid();

            var serializationContext = _biddingHost.Provider.GetRequiredService<ISerializationContext>();

            // Bid, no campaign or exchange should fail
            var resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            campaign = await SetupCampaign();

            // Bid again, should be failed
            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // Add the exchange
            await SetupExchange(exchangeId);

            resp = await MakeBid(bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

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

            // send a loss notification
            resp = await AdvanceBid(serializationContext, bidResponse.Bids.First().Bids.First(), true);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
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
            Assert.IsTrue(campaignBid.CPM > 0, "No bid information");

            return response;
        }

        async Task<HttpResponseMessage> AdvanceBid(ISerializationContext serializationContext, Bid bid, bool win = true, double? amount = null)
        {
            TestContext.WriteLine("Advancing {0} ({1})", bid.Id, win);
            var uri = new Uri(win ? bid.WinUrl.UrlDecode().Replace("${AUCTION_PRICE}", (amount ?? (double)bid.CPM).ToString()) : bid.LossUrl);

            return await _biddingClient.PostAsync(uri.PathAndQuery, null);
        }

        async Task SetupBidderFilters()
        {
            bidderFilter = new BidderFilter
            {
                Id = SequentialGuid.NextGuid().ToString(),
                BidFilter = new BidFilter
                {
                    SiteFilters = new[] { typeof(Site).CreateFilter(FilterType.IN, "Domain", "telefrek") }
                }
            };

            var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();
            var resp = await _orchestrationClient.PostJsonAsync(context, bidderFilter, "/api/filters");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
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
                var resp = await client.PostJsonAsync(context, entity, "api/exchanges");
                Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

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
            campaign = CampaignGenerator.GenerateCampaign();
            var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();
            var resp = await _orchestrationClient.PostJsonAsync(context, campaign, "/api/campaigns");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            creative = await SetupCreative();
            await SetupContents(creative);
            campaign.CreativeIds = new String[] { creative.Id };

            resp = await _orchestrationClient.PutJsonAsync(context, campaign, "/api/campaigns/{0}".FormatWith(campaign.Id));
            Assert.AreEqual(HttpStatusCode.Accepted, resp.StatusCode);

            return campaign;
        }

        async Task<Creative> SetupCreative()
        {
            creative = CreativeGenerator.GenerateCreative();
            var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();
            var resp = await _orchestrationClient.PostJsonAsync(context, creative, "/api/creatives");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            return creative;
        }

        async Task SetupContents(Creative creative)
        {
            Assert.IsTrue(File.Exists("rose.png"), "Failed to find the rose");

            using (var client = _orchestrationHost.CreateClient())
            {
                var context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();

                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    HttpContent httpContent = new StreamContent(File.OpenRead("rose.png"));
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    content.Add(httpContent, "content", "content.png");

                    using (
                       var message =
                           await client.PostAsync("/api/creatives/{0}/content".FormatWith(creative.Id), content))
                    {
                        Assert.AreEqual(HttpStatusCode.Created, message.StatusCode);
                    }
                }

                context = _orchestrationHost.Provider.GetRequiredService<ISerializationContext>();

                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    HttpContent httpContent = new StreamContent(File.OpenRead("clouds.mp4"));
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                    content.Add(httpContent, "content", "content.mp4");

                    using (
                       var message =
                           await client.PostAsync("/api/creatives/{0}/content".FormatWith(creative.Id), content))
                    {
                        Assert.AreEqual(HttpStatusCode.Created, message.StatusCode);
                    }
                }
            }
        }
    }
}