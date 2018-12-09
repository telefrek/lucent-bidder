using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Audio
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
        [SerializationProperty(2, "minduration")]
        public int MinDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "maxduration")]
        public int MaxDuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "protocols")]
        public VideoProtocol[] Protocols { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "startdelay")]
        public StartDelay Delay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "sequence")]
        public int Sequence { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "battr")]
        public BlockedCreative[] BlockedAttributes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "maxextended")]
        public int MaxExtended { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(9, "minbitrate")]
        public int MinBitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "maxbitrate")]
        public int MaxBitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "delivery")]
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
        [SerializationProperty(12, "companionad")]
        public Banner[] CompanionAds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(13, "api")]
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
        [SerializationProperty(21, "maxseq")]
        public int MaxAds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(22, "feed")]
        public FeedType AudioFeedType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(23, "stitched")]
        public bool IsStitched { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(24, "nvol")]
        public VolumeNormalizationMode VolumeNormalization { get; set; }
    }
}