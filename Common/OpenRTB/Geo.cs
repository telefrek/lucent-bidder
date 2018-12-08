using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Geo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "lat")]
        public double Latitude { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>        
        [SerializationProperty(2, "lon")]
        public double Longitude { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "country")]
        public string Country { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "region")]
        public string Region { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "regionfips104")]
        public string RegionFips { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "metro")]
        public string Metro { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "city")]
        public string City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "zip")]
        public string Zip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>        
        [SerializationProperty(9, "type")]
        public GeoType GeoType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(10, "utcoffset")]
        public int UtcOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(11, "accuracy")]
        public int Accuracy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(12, "lastfix")]
        public int LastFixed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(13, "ipservice")]
        public ISP ISP { get; set; }
    }
}