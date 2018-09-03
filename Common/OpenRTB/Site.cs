namespace Lucent.Common.OpenRTB
{
    public class Site
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string[] SiteCategories { get; set; }
        public string[] SectionCategories { get; set; }
        public string[] PageCategories { get; set; }
        public string Page { get; set; }
        public string ReferrerUrl { get; set; }
        public string SearchUrl { get; set; }
        public bool IsMobileOptimized { get; set; }
        public bool IsPrivate { get; set; }
        public Publisher Publisher { get; set; }
        public Content Content { get; set; }
        public string Keywords { get; set; }
    }
}