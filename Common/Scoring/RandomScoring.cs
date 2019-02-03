using System;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Scoring
{
    /// <summary>
    /// Just randomly score stuff... don't use this for real
    /// </summary>
    public class RandomScoring : IScoringService
    {
        Random _rng = new Random();

        /// <inheritdoc/>
        public Task<double> Score(Campaign c, BidRequest request) => Task.FromResult(_rng.NextDouble());
    }
}