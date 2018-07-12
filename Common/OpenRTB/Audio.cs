namespace Lucent.Common.OpenRTB
{
    public class Audio
    {
        public string[] MimeTypes { get; set; }
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
        public VideoProtocol[] Protocols { get; set; }
        public StartDelay Delay { get; set; }
        public int Sequence { get; set; }
        public BlockedCreative[] BlockedAttributes { get; set; }
        public int MaxExtended { get; set; }
        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }
        public ContentDeliveryMethod[] DeliveryMethods { get; set; } = {
        ContentDeliveryMethod.Download,
        ContentDeliveryMethod.Progressive,
        ContentDeliveryMethod.Streaming};
        public Banner[] CompanionAds { get; set; }
        public ApiFramework[] Frameworks { get; set; }
        public CompanionType[] CompanionTypes { get; set; }
        public int MaxAds { get; set; }
        public FeedType AudioFeedType { get; set; }
        public bool IsStitched { get; set; }
        public VolumeNormalizationMode VolumeNormalization { get; set; }
    }
}