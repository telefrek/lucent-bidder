namespace Lucent.Common.OpenRTB
{
    public class Device
    {
        public string UserAgent { get; set; }
        public Geo Geography { get; set; }
        public bool DoNotTrack { get; set; }
        public bool LimitedAdTracking { get; set; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
        public DeviceType DeviceType { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public string HardwareVersion { get; set; }
        public int H { get; set; }
        public int W { get; set; }
        public int PPI { get; set; }
        public double PixelRatio { get; set; }
        public bool SupportJS { get; set; }
        public bool SupportsGeoFetch { get; set; }
        public string FlashVersion { get; set; }
        public string Language { get; set; }
        public string Carrier { get; set; }
        public string MobileCarrierCode { get; set; }
        public ConnectionType NetworkConnection { get; set; }
        public string Id { get; set; }
        public string DeviceIdSHA1 { get; set; }
        public string DeviceIdMD5 { get; set; }
        public string PlatformIdSHA1 { get; set; }
        public string PlatformIdMD5 { get; set; }
        public string MACSHA1 { get; set; }
        public string MACMD5 { get; set; }
    }
}