namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Content
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
        public int Episode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Series { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Season { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Artist { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Genre { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Album { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ISOCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Producer Producer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] Categories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ProductionQuality Quality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ProductionQuality VideoQuality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Context Context { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Context22 {get;set;}

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ContentRating { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string UserRating { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public MediaRating MediaRating { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Keywords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsLive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsDirect { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Length { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsEmbeddable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Data[] Data { get; set; }
    }
}