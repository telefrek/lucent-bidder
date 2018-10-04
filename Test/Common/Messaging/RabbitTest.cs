using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Messaging.Test
{
    [TestClass]
    public class RabbitTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize() => base.TestInitialize();

        [TestMethod]
        public void TestSecondary()
        {
            var factory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var sub = factory.CreateSubscriber<LucentMessage<Campaign>>("campaign-test", 0);
            var pub = factory.CreatePublisher("secondary", "campaign-test");
            var received = false;
            var are = new AutoResetEvent(false);
            sub.OnReceive = (m) =>
            {
                received = true;
                Assert.IsNotNull(m, "Failed to receive message successfully");
                Assert.IsNotNull(m.Body, "Failed to deserialize body");
                foreach (var header in m.Headers)
                    TestContext.WriteLine("{0} : {1}", header.Key, header.Value);
                TestContext.WriteLine("Campaign Name: " + m.Body.Name);
                if (m.Body.Schedule != null)
                {
                    TestContext.WriteLine("Start : {0}", m.Body.Schedule.StartDate);
                    TestContext.WriteLine("End   : {0}", m.Body.Schedule.EndDate);
                }
                are.Set();
            };

            var msg = factory.CreateMessage<LucentMessage<Campaign>>();
            msg.Route = "hello";
            msg.ContentType = "application/x-protobuf";
            msg.Headers.Add("x-lucent-test-header", "something");
            msg.Headers.Add("x-lucent-header-count", 4);
            msg.Body = new Campaign { Id = SequentialGuid.NextGuid().ToString(), Name = "hello", Schedule = new CampaignSchedule { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) } };

            Assert.IsTrue(pub.TryPublish(msg));

            // Wait for some time
            Assert.IsTrue(are.WaitOne(50000));

            Assert.IsTrue(received, "Failed to retrieve message");
        }

        [TestMethod]
        public void TestPubSubMismatchRoute()
        {
            var factory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0, "goodbye");
            var received = false;
            var are = new AutoResetEvent(false);

            sub.OnReceive = (m) =>
            {
                received = true;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
                are.Set();
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route = "hello.world" }), "Failed to send the message");
            // Wait for some time
            Assert.IsFalse(are.WaitOne(5000));

            Assert.IsFalse(received, "Failed to retrieve message");
        }

        [TestMethod]
        public void TestPubSub()
        {
            var factory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0);
            var received = false;
            var are = new AutoResetEvent(false);

            sub.OnReceive = (m) =>
            {
                received = true;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
                are.Set();
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route = "hello.world" }), "Failed to send the message");
            // Wait for some time
            Assert.IsTrue(are.WaitOne(5000));

            Assert.IsTrue(received, "Failed to retrieve message");
        }

        [TestMethod]
        public void TestPubSubBothMixAndMatch()
        {
            var factory = ServiceProvider.GetRequiredService<IMessageFactory>();
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0, "goodbye.*");
            var received = false;
            var count = 0;
            var are = new AutoResetEvent(false);

            sub.OnReceive = (m) =>
            {
                received = true;
                count++;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
                are.Set();
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route = "hello.world" }), "Failed to send the message");
            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route = "goodbye.world.nope" }), "Failed to send the message");
            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route = "goodbye.world" }), "Failed to send the message");

            // Wait for some time
            Assert.IsTrue(are.WaitOne(5000));

            Assert.IsTrue(received, "Failed to retrieve message");
            Assert.AreEqual(1, count, "Should have only gotten one message");
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddMessaging(Configuration);
            services.AddSerialization(Configuration);
            services.AddOpenRTBSerializers();
        }
    }
}