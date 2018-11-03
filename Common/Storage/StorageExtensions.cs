namespace Lucent.Common.Storage
{
    /// <summary>
    /// Extensions to make working with storage objects more sane
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// Get a basic repository from the storage manager
        /// </summary>
        /// <param name="manager"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IBasicStorageRepository<T> GetBasicRepository<T>(this IStorageManager manager)
            where T : IStorageEntity<string>, new()
            => manager.GetRepository<T, string>() as IBasicStorageRepository<T>;
    }
}