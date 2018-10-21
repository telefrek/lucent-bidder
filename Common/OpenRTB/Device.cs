namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class Device
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string UserAgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Geo Geo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool DoNotTrack { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool LimitedAdTracking { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Ipv4 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Ipv6 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Make { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Model { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string OS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string OSVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int PPI { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double PixelRatio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool SupportJS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool SupportsGeoFetch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string FlashVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Carrier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string MobileCarrierCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ConnectionType NetworkConnection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DeviceIdSHA1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DeviceIdMD5 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string PlatformIdSHA1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string PlatformIdMD5 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string MACSHA1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string MACMD5 { get; set; }
    }
}