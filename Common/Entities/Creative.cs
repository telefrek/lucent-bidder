using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lucent.Common.Entities
{
    public class Creative
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        public bool PreserveAspect { get; set; }
        public bool CanScale { get; set; }
        public int BitRate { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public string MimeType { get; set; }
        public string Codec { get; set; }
        public int Duration { get; set; }
        public int Offset { get; set; }
        public string CreativeUri {get;set;}

        public string RawUri { get { return null; } }
    }
}