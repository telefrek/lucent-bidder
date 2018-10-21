namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public enum NoBidReason
    {
        /// <value></value>
        Invalid = 0,
        /// <value></value>
        TechnicalError = 1,
        /// <value></value>
        InvalidRequest = 2,
        /// <value></value>
        KnownWebSpider = 3,
        /// <value></value>
        SuspectedNonHuman = 4,
        /// <value></value>
        CloudDCProxyId = 5,
        /// <value></value>
        UnsupportedDevice = 6,
        /// <value></value>
        BlockedPublisherSite = 7,
        /// <value></value>
        UnmatchedUser = 8,
        /// <value></value>
        DailyReaderCapMet = 9,
        /// <value></value>
        DailyDomainCapMet = 10,
    }
}