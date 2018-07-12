namespace Lucent.Core.Entities.OpenRTB
{
    public enum NoBidReason
    {
        Invalid = 0,
        TechnicalError = 1,
        InvalidRequest = 2,
        KnownWebSpider = 3,
        SuspectedNonHuman = 4,
        CloudDCProxyId = 5,
        UnsupportedDevice = 6,
        BlockedPublisherSite = 7,
        UnmatchedUser = 8,
        DailyReaderCapMet = 9,
        DailyDomainCapMet = 10,
    }
}