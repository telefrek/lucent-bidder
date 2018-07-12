namespace Lucent.Common.OpenRTB
{
    public class User
    {
        public string Id { get; set; }
        public string BuyerId { get; set; }
        public int YOB { get; set; }
        public Gender Gender { get; set; }
        public string[] Keywords { get; set; }
        public string CustomB85 { get; set; }
        public Geo Geo { get; set; }
        public Data[] Data { get; set; }
    }
}