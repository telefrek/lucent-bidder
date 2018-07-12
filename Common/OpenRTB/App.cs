namespace Lucent.Core.Entities.OpenRTB
{
    public class App
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BundleId { get; set; }
        public string Domain { get; set; }
        public string StoreUrl { get; set; }
        public string[] AppCategories { get; set; }
        public string[] SectionCategories { get; set; }
        public string[] PageCategories { get; set; }
        public string Version { get; set; }
        public bool HasPrivacyPolicy { get; set; }
        public bool IsPaidVersion { get; set; }
        public Publisher Publisher { get; set; }
        public Content Content { get; set; }
        public string[] Keywords { get; set; }
    }
}