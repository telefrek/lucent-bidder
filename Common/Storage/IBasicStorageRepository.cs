namespace Lucent.Common.Storage
{
    /// <summary>
    /// Simplified wrapper to remove the need to constantly specify string keys
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasicStorageRepository<T> : IStorageRepository<T, string>
        where T : IStorageEntity<string>, new()
    {

    }
}