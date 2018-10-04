namespace Lucent.Common.Entities
{
    public class CreativeContent
    {
        public string ContentLocation { get; set; }
        public bool PreserveAspect { get; set; }
        public bool CanScale { get; set; }
        public int BitRate { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public string MimeType { get; set; }
        public string Codec { get; set; }
        public int Duration { get; set; }
        public int Offset { get; set; }
        public string CreativeUri { get; set; }
        public string RawUri { get; set; }
    }
}