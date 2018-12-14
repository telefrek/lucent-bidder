using Lucent.Common.Serialization;

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
        [SerializationProperty(1, "mimes")]
        public string[] MimeTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "linearity")]
        public VideoLinearity Linearity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "minduration")]
        public int MinDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "maxduration")]
        public int MaxDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "protocol")]
        public VideoProtocol Protocol { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "w")]
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "h")]
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "startdelay")]
        public StartDelay Delay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(9, "sequence")]
        public int Sequence { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "battr")]
        public BlockedCreative[] BlockedAttributes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "maxextended")]
        public int MaxExtended { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(12, "minbitrate")]
        public int MinBitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(13, "maxbitrate")]
        public int MaxBitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(14, "boxingallowed")]
        public bool BoxingAllowed { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(15, "playbackmethod")]
        public PlaybackMethod[] PlaybackMethods { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(16, "delivery")]
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
        [SerializationProperty(17, "pos")]
        public AdPosition Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(18, "companionad")]
        public Banner[] CompanionAds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(19, "api")]
        public ApiFramework[] Frameworks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(20, "companiontype")]
        public CompanionType[] CompanionTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(21, "protocols")]
        public VideoProtocol[] Protocols { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(22, "companionad_21")]
        public CompanionAd Companion21 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(23, "skip")]
        public bool IsSkippable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(24, "skipmin")]
        public int SkipMinDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(25, "skipafter")]
        public int SkipAfter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(26, "placement")]
        public VideoPlacement Placement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(27, "playbackend")]
        public PlaybackCessation PlaybackEnd { get; set; }
    }
}