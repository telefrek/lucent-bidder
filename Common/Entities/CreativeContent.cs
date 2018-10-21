using System;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class CreativeContent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ContentLocation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool PreserveAspect { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool CanScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int BitRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int H { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string MimeType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Codec { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Offset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string CreativeUri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string RawUri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Func<Impression, bool> Filter { get; set; }
    }
}