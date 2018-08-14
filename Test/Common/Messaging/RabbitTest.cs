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
        public async Task TestPubSub()
        {
            var factory = new RabbitFactory(new TestOptions { Host = "localhost", User = "test", Credentials = "test" });
            var pub = factory.CreatePublisher("blah");
            var sub = factory.CreateSubscriber<LucentMessage>("blah", 0);
            var received = false;

            sub.OnReceive = (m)=>
            {
                received = true;
                var tm = m as LucentMessage;
                Assert.IsNotNull(tm, "Failed to receive message successfully");
                Assert.AreEqual("Hello World", tm.Body, true, "Wrong message returned");
            };

            Assert.IsTrue(pub.TryPublish(new LucentMessage { Body = "Hello World"}), "Failed to send the message");            

            // Wait for some time
            await Task.Delay(5000);

            Assert.IsTrue(received, "Failed to retrieve message");
        }
    }
}