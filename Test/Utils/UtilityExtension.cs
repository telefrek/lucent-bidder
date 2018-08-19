using System;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Bidding;
using Lucent.Common.OpenRTB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Utils.Test
{
    public class UtilityExtension : ILucentExtension
    {
        public void Load(IServiceProvider provider, IConfiguration configuration)
        {
            ExchangeRepository.Instance.RegisterBidder(new TestExchange());
        }
    }

    public class TestExchange : IExchangeBidder
    {
        public string ExchangeId => "test";

        public Task<BidResponse> BidAsync(BidRequest request) =>
            Task.FromResult(new BidResponse
            {
                Id = "Hello",
                CorrelationId = "World",
                NoBidReason = NoBidReason.TechnicalError
            });
    }
}