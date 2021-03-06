using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.OpenRTB.Test
{

    [TestClass]
    public class BidTests : BaseTestClass
    {
        static HttpClient _client = new HttpClient();

        [TestInitialize]
        public override void TestInitialize() => base.TestInitialize();

        [TestMethod]
        [Ignore]
        public async Task TestNoBid()
        {
            var bid = new BidRequest
            {
                Id = Guid.NewGuid().ToString(),
                App = new App
                {
                    Id = "12345",
                    Name = "some app",
                    Domain = "www.google.com",
                    Publisher = new Publisher
                    {
                        Id = "google",
                        Name = "google",
                        Domain = "www.google.com"
                    },
                },
                User = new User
                {
                    Gender = "F",
                    Geo = new Geo
                    {
                        Country = "USA",
                    }
                }
            };
            var response = await MakeBid(bid);
            Assert.IsNotNull(response, "Response shouldn't be null");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "No response should have been sent");
        }

        [TestMethod]
        [Ignore]
        public async Task TestBid()
        {
            var bid = new BidRequest
            {
                Id = Guid.NewGuid().ToString(),
                App = new App
                {
                    Id = "12345",
                    Name = "some app",
                    Domain = "www.google.com",
                    Publisher = new Publisher
                    {
                        Id = "google",
                        Name = "google",
                        Domain = "www.google.com"
                    },
                },
                User = new User
                {
                    Gender = "F",
                    Geo = new Geo
                    {
                        Country = "CAN",
                    }
                }
            };

            var response = await MakeBid(bid);
            Assert.IsNotNull(response, "Response shouldn't be null");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "No response should have been sent");


            var factory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var message = new LucentMessage { Body = "Hello World", CorrelationId = "unittesting", Route = "event.update" };
            using (var pub = factory.CreatePublisher("campaigns"))
            {
                Assert.IsTrue(await pub.TryPublish(message), "Failed to send message");
            }

            // Give it a chance to update
            await Task.Delay(1000);

            bid = new BidRequest
            {
                Id = Guid.NewGuid().ToString(),
                App = new App
                {
                    Id = "12345",
                    Name = "some app",
                    Domain = "www.google.com",
                    Publisher = new Publisher
                    {
                        Id = "google",
                        Name = "google",
                        Domain = "www.google.com"
                    },
                },
                User = new User
                {
                    Gender = "F",
                    Geo = new Geo
                    {
                        Country = "CAN",
                    }
                }
            };

            response = await MakeBid(bid);
            Assert.IsNotNull(response, "Response shouldn't be null");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Response should have been sent");
            var bidResponse = await GetResponse(response);
            Assert.IsNotNull(bidResponse, "BidResponse should not be null");
        }

        [TestMethod]
        [Ignore]
        public async Task TestThrottling()
        {
            var bid = new BidRequest
            {
                Id = Guid.NewGuid().ToString(),
                App = new App
                {
                    Id = "12345",
                    Name = "some app",
                    Domain = "www.google.com",
                    Publisher = new Publisher
                    {
                        Id = "google",
                        Name = "google",
                        Domain = "www.google.com"
                    },
                },
                User = new User
                {
                    Gender = "F",
                    Geo = new Geo
                    {
                        Country = "USA",
                    }
                }
            };

            var tasks = new List<Task<HttpResponseMessage>>();
            var numThreads = 100;

            var maxConcurrent = new SemaphoreSlim(numThreads);

            for (var i = 0; i < 5000; ++i)
            {
                await maxConcurrent.WaitAsync();
                _ = MakeBid(bid).ContinueWith(r => r.Dispose()).ContinueWith(t => maxConcurrent.Release());
            }

            while (maxConcurrent.CurrentCount < numThreads)
                await Task.Delay(100);
        }

        async Task<HttpResponseMessage> MakeBid(BidRequest bid)
        {
            HttpContent content;
            using (var ms = new MemoryStream())
            {
                await ServiceProvider.GetService<ISerializationContext>().WriteTo(bid, ms, true, SerializationFormat.JSON);

                ms.Seek(0, SeekOrigin.Begin);

                content = new StreamContent(ms);

                var test = Encoding.UTF8.GetString(ms.ToArray());

                return await _client.PostAsync("http://localhost:8200/v1/bidder", content);
            }
        }

        async Task<BidResponse> GetResponse(HttpResponseMessage response)
        {
            return await ServiceProvider.GetService<ISerializationContext>().ReadFrom<BidResponse>((await response.Content.ReadAsStreamAsync()), false, SerializationFormat.JSON);
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddLucentServices(Configuration, localOnly: true);
        }
    }
}