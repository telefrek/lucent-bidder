namespace Lucent.Common.Storage
{
    /// <summary>
    /// Manager for storage repositories
    /// </summary>
    public interface IStorageManager
    {
        /// <summary>
        /// Gets a typed repository
        /// </summary>
        /// <typeparam name="T">The type of object to store</typeparam>
        /// <typeparam name="K">The type of key used to retrieve the object</typeparam>
        /// <returns>A storage repository for that type</returns>
        IStorageRepository<T, K> GetRepository<T, K>() where T : IStorageEntity<K>, new();

        /// <summary>
        /// Register a custom repository
        /// </summary>
        /// <param name="repository"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        void RegisterRepository<T, K>(IStorageRepository<T, K> repository) where T : IStorageEntity<K>, new();
    }
}