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
        public StartDelay Delay { get; set; }

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
        public Banner[] CompanionAds { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int MaxAds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public FeedType AudioFeedType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsStitched { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public VolumeNormalizationMode VolumeNormalization { get; set; }
    }
}