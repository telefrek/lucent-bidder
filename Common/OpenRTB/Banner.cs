namespace Lucent.Common.OpenRTB
{
    public class Banner
    {
        public Format[] Formats { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public int WMax { get; set; }
        public int HMax { get; set; }
        public int WMin { get; set; }
        public int HMin { get; set; }
        public BlockedType[] BlockedTypes { get; set; }
        public BlockedCreative[] BlockedCreative { get; set; }
        public AdPosition Position { get; set; }
        public string[] MimeTypes { get; set; }
        public bool IsIFrame { get; set; }
        public ExpandableDirection[] ExpandableDirections { get; set; }
        public ApiFramework[] SupportedApi { get; set; }
        public string Id { get; set; }
        public bool IsBannerConcurrent { get; set; }
    }
}