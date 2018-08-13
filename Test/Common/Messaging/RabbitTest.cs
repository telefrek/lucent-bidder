using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Messaging.Test
{
    [TestClass]
    public class RabbitTests
    {
        [TestMethod]
        public void TestPubSub()
        {
            var pub = RabbitFactory.Instance.GetPublisher("blah");
            var sub = RabbitFactory.Instance.GetSubscriber("blah", "blah");

            Assert.IsTrue(pub.TryPublish("Hello World"), "Failed to send the message");
            var msg = string.Empty;

            Thread.Sleep(5000);

            Assert.IsTrue(sub.TryReceive(out msg), "Failed to retrieve message");
            Assert.AreEqual("Hello World", msg, true, "Mismatched contents");
        }
    }
}