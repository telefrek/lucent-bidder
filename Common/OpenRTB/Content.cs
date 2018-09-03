namespace Lucent.Common.OpenRTB
{

    public class Content
    {
        public string Id { get; set; }
        public int Episode { get; set; }
        public string Title { get; set; }
        public string Series { get; set; }
        public string Season { get; set; }
        public string ArtistCredits { get; set; }
        public string Genre { get; set; }
        public string Album { get; set; }
        public string ISOCode { get; set; }
        public Producer Producer { get; set; }
        public string Url { get; set; }
        public string[] Categories { get; set; }
        public ProductionQuality Quality { get; set; }
        public ProductionQuality VideoQuality { get; set; }
        public Context Context { get; set; }
        public string Context22 {get;set;}
        public string ContentRating { get; set; }
        public string UserRating { get; set; }
        public MediaRating MediaRating { get; set; }
        public string Keywords { get; set; }
        public bool IsLive { get; set; }
        public bool IsDirect { get; set; }
        public int Length { get; set; }
        public string Language { get; set; }
        public bool IsEmbeddable { get; set; }
        public Data[] Data { get; set; }
    }
}