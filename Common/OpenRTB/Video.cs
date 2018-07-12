namespace Lucent.Core.Entities.OpenRTB
{
    public class Video
    {
        public string[] MimeTypes { get; set; }
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
        public VideoProtocol[] Protocols { get; set; }
        public VideoProtocol Protocol { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public StartDelay Delay { get; set; }
        public VideoPlacement Placement { get; set; }
        public VideoLinearity Linearity { get; set; }
        public bool IsSkippable { get; set; }
        public int SkipMinDuration { get; set; }
        public int SkipAfter { get; set; }
        public int Sequence { get; set; } = 1;
        public BlockedCreative[] BlockedAttributes { get; set; }
        public int MaxExtended { get; set; }
        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }
        public bool BoxingAllowed { get; set; } = true;
        public PlaybackMethod[] PlaybackMethods { get; set; }
        public PlaybackCessation PlaybackEnd { get; set; }
        public ContentDeliveryMethod[] DeliveryMethods { get; set; } = {
        ContentDeliveryMethod.Download,
        ContentDeliveryMethod.Progressive,
        ContentDeliveryMethod.Streaming};
        public AdPosition Position { get; set; }
        public Banner[] CompanionAds { get; set; }
        public CompanionAd Companion21 {get;set;}
        public ApiFramework[] Frameworks { get; set; }
        public CompanionType[] CompanionTypes { get; set; }
    }
}