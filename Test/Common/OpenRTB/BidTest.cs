using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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
                    Gender = Gender.Female,
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
                    Gender = Gender.Female,
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
            using(var pub = factory.CreatePublisher("campaigns"))
            {
                Assert.IsTrue(pub.TryPublish(message), "Failed to send message");
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
                    Gender = Gender.Female,
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

        async Task<HttpResponseMessage> MakeBid(BidRequest bid)
        {
            HttpContent content;
            using (var ms = new MemoryStream())
            {
                var serializer = ms.WrapSerializer(ServiceProvider, SerializationFormat.JSON, true);

                using (var writer = serializer.Writer)
                {
                    await writer.WriteAsync(bid);
                    await writer.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);

                content = new StreamContent(ms);

                var test = Encoding.UTF8.GetString(ms.ToArray());

                return await _client.PostAsync("http://localhost:8200/v1/bidder", content);
            }
        }

        async Task<BidResponse> GetResponse(HttpResponseMessage response)
        {
            var serializer = (await response.Content.ReadAsStreamAsync()).WrapSerializer(ServiceProvider, SerializationFormat.JSON, false);

            using(var reader = serializer.Reader)
            {
                if(await reader.HasNextAsync())
                {
                    return await reader.ReadAsAsync<BidResponse>();
                }
            }

            return null;
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddTransient<ISerializationRegistry, SerializationRegistry>();
            services.AddMessaging(Configuration);
        }
    }
}