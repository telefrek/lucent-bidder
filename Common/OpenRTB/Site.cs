namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Site
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
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] SiteCategories { get; set; }

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
        public string Page { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ReferrerUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string SearchUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsMobileOptimized { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsPrivate { get; set; }

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