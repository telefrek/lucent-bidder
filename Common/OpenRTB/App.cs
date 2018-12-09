using Lucent.Common.Serialization;

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
        [SerializationProperty(1, "id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "name")]
        public string Name { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "domain")]
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "cat")]
        public string[] AppCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "sectioncat")]
        public string[] SectionCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "pagecat")]
        public string[] PageCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "ver")]
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "bundle")]
        public string BundleId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(9, "privacypolicy")]
        public bool HasPrivacyPolicy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "paid")]
        public bool IsPaidVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "publisher")]
        public Publisher Publisher { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(12, "content")]
        public Content Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(13, "keywords")]
        public string Keywords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(16, "storeurl")]
        public string StoreUrl { get; set; }
    }
}