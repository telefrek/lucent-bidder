using System.Threading.Tasks;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBidderCache
    {
        /// <summary>
        /// Store something
        /// </summary>
        /// <typeparam name="T"></typeparam>
        Task<bool> TryStore<T>(T instance, string key) where T : class, new();

        /// <summary>
        /// Retrieve it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        Task<T> TryRetrieve<T>(string key) where T : class, new();

        /// <summary>
        /// Update the cache instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        Task<bool> TryUpdate<T>(T instance, string key) where T : class, new();

        /// <summary>
        /// Remove the object
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> TryRemove(string key);
    }
}