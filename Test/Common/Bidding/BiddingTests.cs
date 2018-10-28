using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
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
        public async Task TestSuccessfulBid()
        {
            await SetupBidderFilters();
            await SetupExchange();

            var bid = BidGenerator.GenerateBid();

            RequestDelegate rd = (hc) => { return Task.CompletedTask; };
            var biddingMiddleware = ServiceProvider.CreateInstance<BiddingMiddleware>(rd);
            Assert.IsNotNull(biddingMiddleware, "Failed to create bidding middleware");

            var postbackMiddleware = ServiceProvider.CreateInstance<PostbackMiddleware>(rd);
            Assert.IsNotNull(postbackMiddleware, "Failed to create postback middleware");

            // No campaigns, should fail
            var httpContext = await SetupContext(bid);
            await biddingMiddleware.HandleAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Body should have been read and closed");

            var camp = await SetupCampaign();

            // Campaign setup, but no notification yet
            httpContext = await SetupContext(bid);
            await biddingMiddleware.HandleAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Body should have been read and closed");

            var messageFactory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var publisher = messageFactory.CreatePublisher("bidstate");

            var msg = messageFactory.CreateMessage<LucentMessage<Campaign>>();
            msg.Body = camp;
            msg.Route = "campaign.create";
            msg.ContentType = "application/x-protobuf";

            Assert.IsTrue(publisher.TryPublish(msg), "Failed to publish update");

            // Let the campaign reload
            await Task.Delay(500);

            // Now we should get a bid
            httpContext = await SetupContext(bid);
            await biddingMiddleware.HandleAsync(httpContext);
            Assert.AreEqual(200, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Request body should have been read and closed");
            Assert.IsTrue(httpContext.Response.Body.CanRead, "Response body should be readable");

            // Ensure we filter out an invalid bid
            bid.Impressions.First().Banner.H = 101;
            httpContext = await SetupContext(bid);
            await biddingMiddleware.HandleAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Request body should have been read and closed");


            // Ensure we filter out a blobal bid
            bid.Impressions.First().Banner.H = 100;
            bid.Site = new Site { Domain = "telefrek.com" };
            httpContext = await SetupContext(bid);
            await biddingMiddleware.HandleAsync(httpContext);
            Assert.AreEqual(204, httpContext.Response.StatusCode, "Invalid status code");
            Assert.IsFalse(httpContext.Request.Body.CanRead, "Request body should have been read and closed");
        }

        async Task SetupBidderFilters()
        {
            var fiters = ServiceProvider.GetRequiredService<IStorageManager>().GetRepository<BidderFilter>();
            Assert.IsTrue(await fiters.TryInsert(new BidderFilter
            {
                Id = SequentialGuid.NextGuid().ToString(),
                BidFilter = new BidFilter
                {
                    SiteFilters = new[] { new Filter { FilterType = FilterType.IN, Property = "Domain", Value = "telefrek" } }
                }
            }));
        }

        async Task<HttpContext> SetupContext(BidRequest bid)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream();

            var serializationCtx = ServiceProvider.GetRequiredService<ISerializationContext>();
            await serializationCtx.WrapStream(httpContext.Request.Body, true, SerializationFormat.JSON).Writer.WriteAsync(bid);
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.ContentType = MediaTypeNames.Application.Json;

            var res = Encoding.UTF8.GetString((httpContext.Request.Body as MemoryStream).ToArray());

            httpContext.Response.Body = new MemoryStream();

            return httpContext;
        }

        async Task SetupExchange()
        {
            var registry = ServiceProvider.GetRequiredService<IExchangeRegistry>();
            var exchangePath = Path.Combine(Directory.GetCurrentDirectory(), "exchanges");
            var target = Path.Combine(Directory.GetCurrentDirectory(), "SimpleExchange.dll");
            Assert.IsTrue(File.Exists(target), "Corrupt test environment");

            var newPath = Path.Combine(exchangePath, "SimpleExchange.dll");
            File.Copy(target, newPath);

            while (registry.Exchanges.Count == 0)
                await Task.Delay(200);
        }

        async Task<Campaign> SetupCampaign()
        {
            var campaigns = ServiceProvider.GetRequiredService<IStorageManager>().GetRepository<Campaign>();
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