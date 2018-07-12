namespace Lucent.Core.Entities.OpenRTB
{
    public class Geo
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public GeoType GeoType { get; set; }
        public int Accuracy { get; set; }
        public int LastFixed { get; set; }
        public ISP ISP { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string RegionFips { get; set; }
        public string Metro { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public int UtcOffset { get; set; }
    }
}