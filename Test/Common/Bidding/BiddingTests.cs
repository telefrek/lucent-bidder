using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Bootstrap;
using Lucent.Common.Budget;
using Lucent.Common.Client;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Lucent.Common.Bidding
{
    [TestClass]
    public class BiddingTests
    {
        LucentTestWebHost<OrchestrationStartup> _orchestrationHost;
        LucentTestWebHost<BidderStartup> _biddingHost;
        static HttpClient _orchestrationClient;
        static HttpClient _biddingClient;
        List<Campaign> campaigns = new List<Campaign>();
        Creative creative;
        Guid exchangeId;
        BidderFilter bidderFilter;
        Random _rng = new Random();

        public TestContext TestContext { get; set; }

        [TestCleanup]
        public async Task TestCleanup()
        {
            foreach (var campaign in campaigns)
            {
                var resp = await _orchestrationClient.GetAsync("/api/campaigns/{0}".FormatWith(campaign.Id));
                if (resp.StatusCode != HttpStatusCode.NotFound)
                {
                    var msg = new HttpRequestMessage(HttpMethod.Delete, "/api/campaigns/{0}".FormatWith(campaign.Id));
                    msg.Headers.Add("X-LUCENT-ETAG", resp.Headers.GetValues("X-LUCENT-ETAG").Single());
                    await _orchestrationClient.SendAsync(msg);
                }
            }

            if (creative != null)
            {
                var resp = await _orchestrationClient.GetAsync("/api/creatives/{0}".FormatWith(creative.Id));
                if (resp.StatusCode != HttpStatusCode.NotFound)
                {
                    var msg = new HttpRequestMessage(HttpMethod.Delete, "/api/creatives/{0}".FormatWith(creative.Id));
                    msg.Headers.Add("X-LUCENT-ETAG", resp.Headers.GetValues("X-LUCENT-ETAG").Single());
                    await _orchestrationClient.SendAsync(msg);
                }
            }

            if (exchangeId != null)
            {
                var resp = await _orchestrationClient.GetAsync("/api/exchanges/{0}".FormatWith(exchangeId));
                if (resp.StatusCode != HttpStatusCode.NotFound)
                {
                    var msg = new HttpRequestMessage(HttpMethod.Delete, "/api/exchanges/{0}".FormatWith(exchangeId));
                    msg.Headers.Add("X-LUCENT-ETAG", resp.Headers.GetValues("X-LUCENT-ETAG").Single());
                    await _orchestrationClient.SendAsync(msg);
                }
            }

            if (bidderFilter != null)
            {
                var resp = await _orchestrationClient.GetAsync("/api/filters/{0}".FormatWith(bidderFilter.Id));
                if (resp.StatusCode != HttpStatusCode.NotFound)
                {
                    var msg = new HttpRequestMessage(HttpMethod.Delete, "/api/filters/{0}".FormatWith(bidderFilter.Id));
                    msg.Headers.Add("X-LUCENT-ETAG", resp.Headers.GetValues("X-LUCENT-ETAG").Single());
                    await _orchestrationClient.SendAsync(msg);
                }
            }
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
            _orchestrationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtTokenGenerator().GetBearer(DateTime.Now.AddDays(5)));
            _biddingClient = _biddingHost.CreateClient();

            await Task.CompletedTask;
        }

        class TestClientManger : IClientManager
        {
            public HttpClient OrchestrationClient => _orchestrationClient;
        }

        [TestMethod]
        public void GenerateRSAParams()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    TestContext.WriteLine(JsonConvert.SerializeObject(rsa.ExportParameters(true)));
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        // [TestMethod]
        // public async Task TestMultiple() => await TestMultipleBids("https://east.lucentbid.com", "https://orchestration.lucentbid.com");

        public async Task TestMultipleBids(string bidderUri, string orchestratorUri)
        {
            var sp = ServicePointManager.FindServicePoint(new Uri(bidderUri));
            TestContext.WriteLine("bidder supports pipelining: {0}", sp.SupportsPipelining);

            sp.UseNagleAlgorithm = false;
            sp.ConnectionLimit = 256;

            _orchestrationClient = new HttpClient { BaseAddress = new Uri(orchestratorUri) };

            _orchestrationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtTokenGenerator().GetBearer(DateTime.Now.AddDays(5)));

            _biddingClient = new HttpClient { BaseAddress = new Uri(bidderUri) };

            await SetupBidderFilters(_orchestrationClient, _orchestrationHost.Provider);

            for (var i = 0; i < 10; ++i)
                await SetupCampaign(_orchestrationClient, _orchestrationHost.Provider);

            // Add the exchange
            exchangeId = SequentialGuid.NextGuid();
            await SetupExchange(exchangeId, _orchestrationClient, _orchestrationHost.Provider);

            var ex = await GetExchange(_orchestrationClient, exchangeId.ToString(), _orchestrationHost.Provider.GetRequiredService<ISerializationContext>());
            ex.CampaignIds = campaigns.Where(c => _rng.NextDouble() < .6).Select(c => c.Id).ToArray();

            Assert.IsTrue(await TryUpdateExchange(_orchestrationClient, ex, _orchestrationHost.Provider.GetRequiredService<ISerializationContext>()));

            await Task.Delay(5000);

            var bidCnt = 0L;
            var noBidCnt = 0L;
            var bidWin = 0L;
            var start = DateTime.Now.AddMinutes(-1);
            var total = 0d;
            var tlock = new object();

            var tasks = new List<Task>();
            var numCalls = 10000;//175000;

            for (var i = 0; i < 96; ++i)
            {
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    var myTotal = 0d;
                    var serializationContext = _biddingHost.Provider.GetRequiredService<ISerializationContext>();
                    var sw = new Stopwatch();
                    var rng = new Random();
                    for (var n = 0; n < numCalls; ++n)
                    {
                        try
                        {
                            var bid = BidGenerator.GenerateBid(0);
                            if(n == 0)
                                TestContext.WriteLine(Encoding.UTF8.GetString(await serializationContext.AsBytes(bid, SerializationFormat.JSON)));
                            sw.Start();
                            var resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
                            sw.Stop();
                            if (resp.StatusCode == HttpStatusCode.OK)
                            {
                                Interlocked.Increment(ref bidCnt);
                                if (rng.NextDouble() < 0.07)
                                {
                                    var br = await VerifyBidResponse(resp, serializationContext);
                                    if (br != null)
                                    {
                                        var b = br.Bids[_rng.Next(br.Bids.Length)].Bids.First();
                                        if ((await AdvanceBid(_biddingClient, serializationContext, b, true, b.CPM)).StatusCode == HttpStatusCode.OK)
                                        {
                                            myTotal += b.CPM;
                                            Interlocked.Increment(ref bidWin);
                                            if (rng.NextDouble() < rng.NextDouble() * .07)
                                            {
                                                await PostbackAction(_biddingClient, bid, b, "install");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                Interlocked.Increment(ref noBidCnt);
                        }
                        catch (Exception e)
                        {
                            TestContext.WriteLine("Error : {0}", e.ToString());
                        }
                    }

                    lock (tlock)
                    {
                        total += myTotal;
                    }

                    TestContext.WriteLine("Avg Time : {0:0.000} ms", (sw.ElapsedTicks * 1000d / Stopwatch.Frequency) / numCalls);
                }).Unwrap());

                // Delay adding new users
                await Task.Delay(2000);
            }

            // Wait for the tasks to end
            await Task.WhenAll(tasks);
            TestContext.WriteLine("Bid   {0:#,##0} ({1:#,##0.00})", bidCnt, 1.0 * bidCnt / (bidCnt + noBidCnt));
            TestContext.WriteLine("Wins  {0:#,##0} ({1:#,##0.00})", bidWin, 1.0 * bidWin / bidCnt);
            TestContext.WriteLine("NoBid {0:#,##0} ({1:#,##0.00})", noBidCnt, 1.0 * noBidCnt / (bidCnt + noBidCnt));

            var end = DateTime.Now.AddSeconds(1);

            Assert.IsTrue(bidCnt > 0, "Failed to make any bids");

            var summary = await GetTestSummary(_orchestrationClient, _biddingHost.Provider, exchangeId.ToString(), start, end);
            Assert.IsNotNull(summary);
            TestContext.WriteLine("Total spend : {0}", total / 1000d);
            TestContext.WriteLine("Exchange: {0}", Encoding.UTF8.GetString(await _biddingHost.Provider.GetRequiredService<ISerializationContext>().AsBytes(summary, SerializationFormat.JSON)));

            foreach (var campaign in campaigns)
            {
                summary = await GetTestSummary(_orchestrationClient, _biddingHost.Provider, campaign.Id, start, end);
                TestContext.WriteLine("Campaign ({1}): {0}", Encoding.UTF8.GetString(await _biddingHost.Provider.GetRequiredService<ISerializationContext>().AsBytes(summary, SerializationFormat.JSON)), campaign.Id);
            }
        }


        [TestMethod]
        public async Task TestSuccessfulBid()
        {
            await SetupBidderFilters(_orchestrationClient, _orchestrationHost.Provider);
            exchangeId = SequentialGuid.NextGuid();
            var bid = BidGenerator.GenerateBid(0);

            var serializationContext = _biddingHost.Provider.GetRequiredService<ISerializationContext>();

            // Bid, no campaign or exchange should fail
            var resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            var campaign = await SetupCampaign(_orchestrationClient, _orchestrationHost.Provider);

            // Bid again, should be failed, no exchange
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // Add the exchange
            await SetupExchange(exchangeId, _orchestrationClient, _orchestrationHost.Provider);

            // Bid again, no exchange budget
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
            await Task.Delay(1000);

            // Bid again, no campaigns on exchange
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            var ex = await GetExchange(_orchestrationClient, exchangeId.ToString(), serializationContext);
            ex.CampaignIds = campaigns.Select(c => c.Id).ToArray();

            Assert.IsTrue(await TryUpdateExchange(_orchestrationClient, ex, serializationContext));

            await Task.Delay(1000);

            // Bid again, no campaign budget
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
            await Task.Delay(1000);

            // Bid again, no campaign budget
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
            await Task.Delay(1000);

            // Should work
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            // Verify the response
            var bidResponse = await VerifyBidResponse(resp, serializationContext);
            Assert.IsNotNull(bidResponse);

            // Ensure we filter out an invalid bid
            bid.Impressions.First().Banner.H = 101;
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // Ensure we filter out a global bid
            bid.Impressions.First().Banner.H = 100;
            bid.Site = new Site { Domain = "telefrek.com" };
            resp = await MakeBid(_biddingClient, bid, serializationContext, exchangeId);
            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            // send a loss notification
            resp = await AdvanceBid(_biddingClient, serializationContext, bidResponse.Bids[_rng.Next(bidResponse.Bids.Length)].Bids.First(), true);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        async Task<HttpResponseMessage> MakeBid(HttpClient biddingClient, BidRequest bid, ISerializationContext serializationContext, Guid exchangeId)
        {
            using (var ms = new MemoryStream())
            {
                using (var gz = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    await serializationContext.WriteTo(bid, gz, true, SerializationFormat.JSON);
                    await gz.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);
                using (var bidContent = new StreamContent(ms, 4096))
                {
                    bidContent.Headers.Add("Content-Type", "application/json");
                    bidContent.Headers.Add("Content-Encoding", "gzip");
                    var request = new HttpRequestMessage(HttpMethod.Post, "/v1/bidder?exchg={0}".FormatWith(exchangeId));
                    request.Content = bidContent;
                    request.Headers.Add("Accept-Encoding", "gzip");
                    return await biddingClient.SendAsync(request);
                }
            }
        }

        async Task<BidResponse> VerifyBidResponse(HttpResponseMessage responseMessage, ISerializationContext serializationContext)
        {
            var response = await serializationContext.ReadFrom<BidResponse>(await responseMessage.Content.ReadAsStreamAsync(), false, SerializationFormat.JSON | SerializationFormat.COMPRESSED);
            Assert.IsNotNull(response, "Bid response should not be null");
            Assert.IsNotNull(response.Bids, "Bids should be present");
            var seatBid = response.Bids.First();
            Assert.IsNotNull(seatBid.Bids, "Bids must be part of seatbid");
            Assert.IsTrue(seatBid.Bids.All(b => campaigns.Any(c => c.Id == b.CampaignId) && b.CPM > 0d), "Campaign should exist in test set only and have a valid CPM");

            return response;
        }

        async Task<HttpResponseMessage> AdvanceBid(HttpClient biddingClient, ISerializationContext serializationContext, Bid bid, bool win = true, double? amount = null)
        {
            var uri = new Uri(win ? bid.WinUrl.UrlDecode().Replace("${AUCTION_PRICE}", Math.Round(amount ?? (double)bid.CPM, 4).ToString()) : bid.LossUrl);

            return await biddingClient.PostAsync(uri.PathAndQuery, null);
        }

        async Task PostbackAction(HttpClient biddingClient, BidRequest request, Bid bid,
            string action)
        {
            var campaign = campaigns.First(c => c.Id == bid.CampaignId);
            var context = new BidContext
            {
                Campaign = campaign,
                CampaignId = Guid.Parse(campaign.Id),
                BidId = Guid.Parse(bid.Id),
                ExchangeId = exchangeId,
                BaseUri = new UriBuilder(biddingClient.BaseAddress),
                Request = request,
                RequestId = request.Id,
                BidDate = DateTime.Now
            };

            var lctx = context.GetOperationString(BidOperation.Action);

            await biddingClient.PostAsync("/v1/postback?{2}={0}&action={1}".FormatWith(lctx, action, QueryParameters.LUCENT_BID_CONTEXT_PARAMETER), null);
        }

        async Task ClickAd(HttpClient biddingClient, ISerializationContext serializationContext, Bid bid)
        {
            await Task.CompletedTask;
        }

        async Task<LedgerSummary> GetTestSummary(HttpClient orchestrationClient, IServiceProvider serviceProvider, string entity, DateTime start, DateTime end)
        {
            var serializationContext = serviceProvider.GetRequiredService<ISerializationContext>();
            var resp = await orchestrationClient.GetAsync("/api/ledger/{0}?start={1}&end={2}".FormatWith(entity, start.ToString("o"), end.ToString("o")));
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            var contents = await serializationContext.ReadArrayFrom<LedgerSummary>(await resp.Content.ReadAsStreamAsync(), false, SerializationFormat.JSON);
            return contents.FirstOrDefault();
        }

        async Task SetupBidderFilters(HttpClient orchestrationClient, IServiceProvider serviceProvider)
        {
            bidderFilter = new BidderFilter
            {
                Id = SequentialGuid.NextGuid().ToString(),
                BidFilter = new BidFilter
                {
                    SiteFilters = new[] { typeof(Site).CreateFilter(FilterType.IN, "Domain", "telefrek") }
                }
            };

            var serializationContext = serviceProvider.GetRequiredService<ISerializationContext>();
            var resp = await orchestrationClient.PostJsonAsync(serializationContext, bidderFilter, "/api/filters");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            var filters = await GetFilters(orchestrationClient, serializationContext);
            Assert.IsTrue(filters.Any(f => f.Id == bidderFilter.Id), "Filter not in get all");
            Assert.IsNotNull(await GetFilter(orchestrationClient, bidderFilter.Id, serializationContext), "Failed to get bid filter");
        }

        public async Task<List<BidderFilter>> GetFilters(HttpClient orchestrationClient, ISerializationContext serializationContext)
        {
            var filters = new List<BidderFilter>();
            var resp = await orchestrationClient.GetAsync("api/filters");
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                var contents = await serializationContext.ReadArrayFrom<BidderFilter>(await resp.Content.ReadAsStreamAsync(), false, SerializationFormat.JSON);

                if (contents != null)
                    filters.AddRange(contents);
            }
            return filters;
        }

        public async Task<BidderFilter> GetFilter(HttpClient orchestrationClient, String filterId, ISerializationContext serializationContext)
        {
            var resp = await orchestrationClient.GetAsync("api/filters/" + filterId);
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                return await serializationContext.ReadFrom<BidderFilter>(await resp.Content.ReadAsStreamAsync(), false, SerializationFormat.JSON);
            }

            return null;
        }

        public async Task<Exchange> GetExchange(HttpClient orchestrationClient, String exchangeId, ISerializationContext serializationContext)
        {
            var resp = await orchestrationClient.GetAsync("api/exchanges/" + exchangeId);
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                var tags = (IEnumerable<string>)null;
                var etag = "";
                if (resp.Headers.TryGetValues("X-LUCENT-ETAG", out tags))
                    etag = tags.FirstOrDefault() ?? "";
                var exchange = await serializationContext.ReadFrom<Exchange>(await resp.Content.ReadAsStreamAsync(), false, SerializationFormat.JSON);
                exchange.ETag = etag;
                return exchange;
            }

            return null;
        }

        public async Task<bool> TryUpdateExchange(HttpClient orchestrationClient, Exchange exchange, ISerializationContext serializationContext)
        {
            var resp = await orchestrationClient.PutJsonAsync(serializationContext, exchange, "api/exchanges/" + exchange.Id.ToString(), exchange.ETag);
            return (resp.StatusCode == HttpStatusCode.Accepted);
        }

        async Task SetupExchange(Guid id, HttpClient orchestrationClient, IServiceProvider serviceProvider)
        {
            var target = Path.Combine(Directory.GetCurrentDirectory(), "SimpleExchange.dll");
            Assert.IsTrue(File.Exists(target), "Corrupt test environment");

            var entity = new Exchange
            {
                Name = "test",
                Id = id,
                LastCodeUpdate = DateTime.UtcNow,
                BudgetSchedule = new BudgetSchedule
                {
                    ScheduleType = ScheduleType.Aggressive,
                    HourlyCap = 50,
                    DailyCap = 300,
                }
            };

            var context = serviceProvider.GetRequiredService<ISerializationContext>();
            var resp = await orchestrationClient.PostJsonAsync(context, entity, "api/exchanges");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            using (var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                content.Add(new StreamContent(File.OpenRead(target)), "exchange", "simple.dll");

                using (
                   var message =
                       await orchestrationClient.PutAsync("/api/exchanges/{0}".FormatWith(id), content))
                {
                    Assert.AreEqual(HttpStatusCode.Accepted, message.StatusCode);
                }
            }
        }

        async Task<Campaign> SetupCampaign(HttpClient orchestrationClient, IServiceProvider serviceProvider)
        {
            var campaign = CampaignGenerator.GenerateCampaign();
            var context = serviceProvider.GetRequiredService<ISerializationContext>();
            TestContext.WriteLine(Encoding.UTF8.GetString(await context.AsBytes(campaign, SerializationFormat.JSON)));
            var resp = await orchestrationClient.PostJsonAsync(context, campaign, "/api/campaigns");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            var tags = (IEnumerable<string>)null;
            var etag = "";
            if (resp.Headers.TryGetValues("X-LUCENT-ETAG", out tags))
                etag = tags.FirstOrDefault() ?? "";

            if (creative == null)
            {
                creative = await SetupCreative(orchestrationClient, serviceProvider);
                await SetupContents(creative, orchestrationClient, serviceProvider);
            }
            campaign.CreativeIds = new String[] { creative.Id };

            resp = await orchestrationClient.PutJsonAsync(context, campaign, "/api/campaigns/{0}".FormatWith(campaign.Id), etag);
            Assert.AreEqual(HttpStatusCode.Accepted, resp.StatusCode);
            campaigns.Add(campaign);
            return campaign;
        }

        async Task<bool> TryAddCampaign(HttpClient orchestrationClient, IServiceProvider serviceProvider, string campaignId)
        {
            return await Task.FromResult(false);
        }

        async Task<Creative> SetupCreative(HttpClient orchestrationClient, IServiceProvider serviceProvider)
        {
            creative = CreativeGenerator.GenerateCreative();
            var context = serviceProvider.GetRequiredService<ISerializationContext>();
            TestContext.WriteLine(Encoding.UTF8.GetString(await context.AsBytes(creative, SerializationFormat.JSON)));
            var resp = await orchestrationClient.PostJsonAsync(context, creative, "/api/creatives");
            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            return creative;
        }

        async Task SetupContents(Creative creative, HttpClient orchestrationClient, IServiceProvider serviceProvider)
        {
            Assert.IsTrue(File.Exists("rose.png"), "Failed to find the rose");


            var context = serviceProvider.GetRequiredService<ISerializationContext>();

            using (var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                HttpContent httpContent = new StreamContent(File.OpenRead("rose.png"));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                content.Add(httpContent, "content", "content.png");

                using (
                   var message =
                       await orchestrationClient.PostAsync("/api/creatives/{0}/content".FormatWith(creative.Id), content))
                {
                    Assert.AreEqual(HttpStatusCode.Created, message.StatusCode);
                }
            }

            context = serviceProvider.GetRequiredService<ISerializationContext>();

            using (var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                HttpContent httpContent = new StreamContent(File.OpenRead("clouds.mp4"));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                content.Add(httpContent, "content", "content.mp4");

                using (
                   var message =
                       await orchestrationClient.PostAsync("/api/creatives/{0}/content".FormatWith(creative.Id), content))
                {
                    Assert.AreEqual(HttpStatusCode.Created, message.StatusCode);
                }
            }
        }
    }
}