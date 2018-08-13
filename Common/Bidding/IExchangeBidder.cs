using System.Threading.Tasks;
using Lucent.Common.Entities.OpenRTB;

namespace Lucent.Common.Bidding
{
    public interface IExchangeBidder
    {
        Task<BidResponse> BidAsync(BidRequest request);
    }
}