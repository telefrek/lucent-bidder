namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Video
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] MimeTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int MinDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int MaxDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public VideoProtocol[] Protocols { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public VideoProtocol Protocol { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public StartDelay Delay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public VideoPlacement Placement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public VideoLinearity Linearity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsSkippable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int SkipMinDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int SkipAfter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Sequence { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BlockedCreative[] BlockedAttributes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int MaxExtended { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int MinBitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int MaxBitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool BoxingAllowed { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public PlaybackMethod[] PlaybackMethods { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public PlaybackCessation PlaybackEnd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ContentDeliveryMethod[] DeliveryMethods { get; set; } =
        {
            ContentDeliveryMethod.Download,
            ContentDeliveryMethod.Progressive,
            ContentDeliveryMethod.Streaming
        };

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public AdPosition Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Banner[] CompanionAds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public CompanionAd Companion21 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ApiFramework[] Frameworks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public CompanionType[] CompanionTypes { get; set; }
    }
}