using System;
using Lucent.Common;
using Lucent.Common.Bidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent
{
    [TestClass]
    public class ContextTest
    {
        [TestMethod]
        public void TestMe()
        {
            var bc = new BidContext
            {
                ExchangeId = SequentialGuid.NextGuid(),
                CampaignId = SequentialGuid.NextGuid(),
                BidId = SequentialGuid.NextGuid(),
                RequestId = "12345",
                CPM = 1.0,
                Operation = BidOperation.Win,
                BidDate = DateTime.UtcNow
            };

            var str = bc.GetOperationString(BidOperation.Win);

            var bc2 = BidContext.Parse(str);
            Assert.IsNotNull(bc2);

            var str2 = bc2.GetOperationString(BidOperation.Win);
            Assert.AreEqual(str, str2);
        }
    }
}