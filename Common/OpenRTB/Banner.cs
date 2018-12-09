using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Banner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "w")]
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "h")]
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "pos")]
        public AdPosition Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "btype")]
        public BlockedType[] BlockedTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "battr")]
        public BlockedCreative[] BlockedCreative { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "mimes")]
        public string[] MimeTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "topframe")]
        public bool IsIFrame { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(9, "expdir")]
        public ExpandableDirection[] ExpandableDirections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "api")]
        public ApiFramework[] SupportedApi { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "wmax")]
        public int WMax { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(12, "hmax")]
        public int HMax { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(13, "wmin")]
        public int WMin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(14, "hmin")]
        public int HMin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(15, "format")]
        public Format[] Formats { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(16, "vcm")]
        public bool IsBannerConcurrent { get; set; }
    }
}