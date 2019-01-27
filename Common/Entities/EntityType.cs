namespace Lucent.Common.Entities
{
    /// <summary>
    /// Enum for quickly getting types passed across messages
    /// </summary>
    public enum EntityType
    {
        /// <value></value>
        Unknown = 0,
        /// <value></value>
        Campaign = 1,
        /// <value></value>
        Creative = 2,
        /// <value></value>
        CreativeContent = 3,
        /// <value></value>
        Exchange = 4,
        /// <value></value>
        Ledger = 5,
        /// <value></value>
        BidderFilter = 6,
    }
}