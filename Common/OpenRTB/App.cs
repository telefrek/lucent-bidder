namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class App
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BundleId { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string StoreUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] AppCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] SectionCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] PageCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool HasPrivacyPolicy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsPaidVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Publisher Publisher { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Content Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Keywords { get; set; }
    }
}