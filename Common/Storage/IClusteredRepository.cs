namespace Lucent.Common.Storage
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IClusteredRepository<T> : IStorageRepostory<T>
        where T : IClusteredStorageEntity
    {

    }
}