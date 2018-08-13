using Lucent.Common.Entities.OpenRTB;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Represents the bidder confidence in winning this impression
    /// </summary>
    public class BidConfidence
    {
        public Impression Impression { get; set; }
        public double Confidence { get; set; }
    }
}