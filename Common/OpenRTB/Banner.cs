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
        public Format[] Formats { get; set; }

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
        public int WMax { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int HMax { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int WMin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int HMin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BlockedType[] BlockedTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BlockedCreative[] BlockedCreative { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public AdPosition Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] MimeTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsIFrame { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ExpandableDirection[] ExpandableDirections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ApiFramework[] SupportedApi { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsBannerConcurrent { get; set; }
    }
}