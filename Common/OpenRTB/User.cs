namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BuyerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int YOB { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Gender Gender { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Keywords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string CustomB85 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Geo Geo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Data[] Data { get; set; }
    }
}