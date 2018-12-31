using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
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
    public class BiddingTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            if (Directory.Exists(exchangePath))
                Directory.Delete(exchangePath, true);
            Directory.CreateDirectory(exchangePath);

            base.TestInitialize();
        }

        [TestMethod]
        //[Ignore]
        public async Task TestSuccessfulBid()
        {
            var serializationContext = ServiceProvider.GetRequiredService<ISerializationContext>();
            await SetupBidderFilters();
            var exchangeId = SequentialGuid.NextGuid();
            await SetupExchange(exchangeId);

            var bid = BidGenerator.GenerateBid();

            var rd = new RequestDelegate(async (hc) => { await Task.CompletedTask; });

            var biddingMiddleware = ServiceProvider.CreateInstance<BiddingMiddleware>(rd);
            Assert.IsNotNull(biddingMiddleware, "Failed to create bidding middleware");

            var postbackMiddleware = ServiceProvider.CreateInstance<PostbackMiddleware>(rd);
            Assert.IsNotNull(postbackMiddleware, "Failed to create postback middleware");

            // No campaigns, should fail
            var httpContext = await SetupContext(bid, exchangeId);
            await biddingMiddleware.InvokeAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Body should have been read and closed");

            var campaign = await SetupCampaign();

            // Campaign setup, but no notification yet
            httpContext = await SetupContext(bid, exchangeId);
            await biddingMiddleware.InvokeAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Body should have been read and closed");

            var messageFactory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var publisher = messageFactory.CreatePublisher("bidding");

            var msg = messageFactory.CreateMessage<EntityEventMessage>();
            msg.Body = new EntityEvent
            {
                EntityType = EntityType.Campaign,
                EntityId = campaign.Id,
                EventType = EventType.EntityAdd
            };

            msg.Route = "campaign";
            msg.ContentType = "application/x-protobuf";

            Assert.IsTrue(await publisher.TryBroadcast(msg), "Failed to broadcast update");

            // Let the campaign reload
            await Task.Delay(500);

            // Now we should get a bid
            httpContext = await SetupContext(bid, exchangeId);
            await biddingMiddleware.InvokeAsync(httpContext);
            Assert.AreEqual(200, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Request body should have been read and closed");
            Assert.IsTrue(httpContext.Response.Body.CanRead, "Response body should be readable");

            // Verify the response
            var bidResponse = await VerifyBidResponse(httpContext, serializationContext, campaign);

            // Simulate win/loss
            var winBid = bidResponse.Bids.First().Bids.First();
            var billContext = await SetupBillContext(winBid);

            await postbackMiddleware.HandleAsync(billContext);
            Assert.AreEqual(200, httpContext.Response.StatusCode);

            // Ensure we filter out an invalid bid
            bid.Impressions.First().Banner.H = 101;
            httpContext = await SetupContext(bid, exchangeId);
            await biddingMiddleware.InvokeAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Request body should have been read and closed");

            // Ensure we filter out a global bid
            bid.Impressions.First().Banner.H = 100;
            bid.Site = new Site { Domain = "telefrek.com" };
            httpContext = await SetupContext(bid, exchangeId);
            await biddingMiddleware.InvokeAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Request body should have been read and closed");
        }

        async Task<BidResponse> VerifyBidResponse(HttpContext httpContext, ISerializationContext serializationContext, Campaign campaign)
        {
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

            var contents = Encoding.UTF8.GetString((httpContext.Response.Body as MemoryStream).ToArray());

            var response = await serializationContext.ReadFrom<BidResponse>(httpContext.Response.Body, false, SerializationFormat.JSON);
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
            var fiters = ServiceProvider.GetRequiredService<IStorageManager>().GetRepository<BidderFilter, string>();
            Assert.IsTrue(await fiters.TryInsert(new BidderFilter
            {
                Id = SequentialGuid.NextGuid().ToString(),
                BidFilter = new BidFilter
                {
                    SiteFilters = new[] { typeof(Site).CreateFilter(FilterType.IN, "Domain", "telefrek") }
                }
            }));
        }

        async Task<HttpContext> SetupContext(BidRequest bid, Guid exchangeId)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream();
            httpContext.Request.Host = new HostString("localhost");
            httpContext.Request.Scheme = "https";
            httpContext.Request.Path = "/v1/bidder";
            httpContext.Request.QueryString = new QueryString("?exchg={0}".FormatWith(exchangeId));

            var serializationContext = ServiceProvider.GetRequiredService<ISerializationContext>();
            await serializationContext.WriteTo(bid, httpContext.Request.Body, true, SerializationFormat.JSON);
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.ContentType = MediaTypeNames.Application.Json;

            httpContext.Response.Body = new MemoryStream();

            return httpContext;
        }

        async Task<HttpContext> SetupWinContext(Bid bid)
        {
            Uri uri;
            Uri.TryCreate(bid.WinUrl, UriKind.Absolute, out uri);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = MediaTypeNames.Application.Json;

            httpContext.Request.Host = new HostString(uri.Host);
            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Path = uri.AbsolutePath;
            httpContext.Request.QueryString = new QueryString(uri.Query);

            return await Task.FromResult(httpContext);
        }

        async Task<HttpContext> SetupBillContext(Bid bid)
        {
            Uri uri;
            Uri.TryCreate(bid.BillingUrl, UriKind.Absolute, out uri);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = MediaTypeNames.Application.Json;

            httpContext.Request.Host = new HostString(uri.Host);
            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Path = uri.AbsolutePath;
            httpContext.Request.QueryString = new QueryString(uri.Query);

            return await Task.FromResult(httpContext);
        }

        async Task<HttpContext> SetupLossContext(Bid bid)
        {
            Uri uri;
            Uri.TryCreate(bid.LossUrl, UriKind.Absolute, out uri);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = MediaTypeNames.Application.Json;

            httpContext.Request.Host = new HostString(uri.Host);
            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Path = uri.AbsolutePath;
            httpContext.Request.QueryString = new QueryString(uri.Query);

            return await Task.FromResult(httpContext);
        }

        async Task SetupExchange(Guid id)
        {
            var registry = ServiceProvider.GetRequiredService<IExchangeRegistry>();
            await registry.Initialize();
            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            var target = Path.Combine(Directory.GetCurrentDirectory(), "SimpleExchange.dll");
            Assert.IsTrue(File.Exists(target), "Corrupt test environment");

            var manager = ServiceProvider.GetRequiredService<IStorageManager>();
            var repo = manager.GetRepository<Exchange, Guid>();
            var entity = new Exchange
            {
                Name = "test",
                Id = id,
                Code = new MemoryStream(await File.ReadAllBytesAsync(target)),
                LastCodeUpdate = DateTime.UtcNow,
            };

            var res = await repo.TryInsert(entity);
            Assert.IsTrue(res, "Failed to create exchange");
            var pub = ServiceProvider.GetRequiredService<IMessageFactory>().CreatePublisher("exchanges");
            var msg = ServiceProvider.GetRequiredService<IMessageFactory>().CreateMessage<EntityEventMessage>();

            var asm = AssemblyLoadContext.Default.LoadFromStream(entity.Code);
            if (asm != null)
            {
                var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(AdExchange).IsAssignableFrom(t));
                if (exchgType != null)
                {
                    var exchg = ServiceProvider.CreateInstance(exchgType) as AdExchange;
                    if (exchg != null)
                    {
                        exchg.ExchangeId = (Guid)(object)entity.Id;
                        await exchg.Initialize(ServiceProvider);
                        entity.Instance = exchg;
                    }
                }
            }

            msg.Body = new EntityEvent
            {
                EntityId = id.ToString(),
                EventType = EventType.EntityAdd,
                EntityType = EntityType.Exchange,
            };
            msg.Route = "update";
            Assert.IsTrue(await pub.TryBroadcast(msg), "failed to broadcast message");

            for (var i = 0; i < 10; ++i)
            {
                if (registry.HasExchange(id))
                    return;
                await Task.Delay(250);
            }

            Assert.Fail("Failed to load the exchange");
        }

        async Task<Campaign> SetupCampaign()
        {
            var campaigns = ServiceProvider.GetRequiredService<IStorageManager>().GetBasicRepository<Campaign>();
            var camp = CampaignGenerator.GenerateCampaign();
            Assert.IsTrue(await campaigns.TryInsert(camp), "Failed to insert campaign");

            return camp;
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddLucentServices(Configuration, localOnly: true, includeBidder: true);
        }
    }
}