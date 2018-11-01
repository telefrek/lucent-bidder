using System;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Tracking for a campaign budget
    /// </summary>
    public class Budget : IBasicStorageEntity
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
        public string ETag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime Updated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Version { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double Remaining { get; set; } = 0d;
    }
}