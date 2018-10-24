namespace Lucent.Common.Storage
{
    /// <summary>
    /// Clustering through a second id
    /// </summary>
    public interface IClusteredStorageEntity : IStorageEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        string SecondaryId { get; set; }
    }
}