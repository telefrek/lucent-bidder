using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Messaging.Test
{
    [TestClass]
    public class RabbitTests
    {
        class TestOptions : RabbitConfiguration, IOptions<RabbitConfiguration>
        {
            public RabbitConfiguration Value => this;
        }
                
        [TestMethod]
        public void TestPubSubMismatchRoute()
        {
            IMessageFactory factory = new RabbitFactory(new TestOptions { Host = "localhost", User = "test", Credentials = "test" });
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0, "goodbye");
            var received = false;
            var are = new AutoResetEvent(false);

            sub.OnReceive = (m)=>
            {
                received = true;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
                are.Set();
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route="hello.world"}), "Failed to send the message");            
            // Wait for some time
            Assert.IsFalse(are.WaitOne(5000));

            Assert.IsFalse(received, "Failed to retrieve message");
        }

        [TestMethod]
        public void TestPubSub()
        {
            IMessageFactory factory = new RabbitFactory(new TestOptions { Host = "localhost", User = "test", Credentials = "test" });
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0);
            var received = false;
            var are = new AutoResetEvent(false);

            sub.OnReceive = (m)=>
            {
                received = true;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
                are.Set();
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route="hello.world"}), "Failed to send the message");            
            // Wait for some time
            Assert.IsTrue(are.WaitOne(5000));

            Assert.IsTrue(received, "Failed to retrieve message");
        }

        [TestMethod]
        public void TestPubSubBothMixAndMatch()
        {
            IMessageFactory factory = new RabbitFactory(new TestOptions { Host = "localhost", User = "test", Credentials = "test" });
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0, "goodbye.*");
            var received = false;
            var count = 0;
            var are = new AutoResetEvent(false);

            sub.OnReceive = (m)=>
            {
                received = true;
                count++;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
                are.Set();
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route="hello.world"}), "Failed to send the message");   
            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route="goodbye.world.nope"}), "Failed to send the message");            
            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World", Route="goodbye.world"}), "Failed to send the message");

            // Wait for some time
            Assert.IsTrue(are.WaitOne(5000));

            Assert.IsTrue(received, "Failed to retrieve message");
            Assert.AreEqual(1, count, "Should have only gotten one message");
        }
    }
}