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
        public double Latitude { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double Longitude { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public GeoType GeoType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Accuracy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int LastFixed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ISP ISP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Country { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Region { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string RegionFips { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Metro { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Zip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int UtcOffset { get; set; }
    }
}