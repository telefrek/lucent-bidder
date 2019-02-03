using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Scoring
{
    /// <summary>
    /// Represents a scoring service available for invocation
    /// </summary>
    public interface IScoringService
    {
        /// <summary>
        /// Score the campaign compared to the request
        /// </summary>
        /// <param name="c"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<double> Score(Campaign c, BidRequest request);
    }
}