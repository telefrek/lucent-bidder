using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Bidding
{
    public sealed class ExchangeRepository
    {
        ConcurrentBag<IExchangeBidder> _bidders = new ConcurrentBag<IExchangeBidder>();

        public static ExchangeRepository Instance = new ExchangeRepository();

        protected ExchangeRepository()
        {

        }

        public void RegisterBidder(IExchangeBidder bidder)
        {
            _bidders.Add(bidder);
        }

        public async Task<BidResponse> BidAsync(BidRequest request)
        {
            foreach(var bidder in _bidders)
                return await bidder.BidAsync(request);

            return null;
        }
    }
}