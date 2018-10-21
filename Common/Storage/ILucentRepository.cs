using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Representation of a generic data repository
    /// </summary>
    /// <typeparam name="T">The type of object managed by the repository</typeparam>
    public interface ILucentRepository<T>
    where T : IStorageEntity
    {
        /// <summary>
        /// Gets all the objects in the collection
        /// </summary>
        /// <returns></returns>
        Task<ICollection<T>> Get();

        /// <summary>
        /// Gets a single object from the collection that has the matching id
        /// </summary>
        /// <param name="id">The object id to search for</param>
        /// <returns></returns>
        Task<T> Get(string id);

        /// <summary>
        /// Tries to insert the object into the collection
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <returns>True if inserted</returns>
        Task<bool> TryInsert(T obj);

        /// <summary>
        /// Tries to update te object in the collection
        /// </summary>
        /// <param name="obj">The object to update</param>
        /// <returns>True if the object was updated</returns>
        Task<bool> TryUpdate(T obj);

        /// <summary>
        /// Tries to delete the object from the collection
        /// </summary>
        /// <param name="obj">The object to delete</param>
        /// <returns>True if the object was deleted successfully</returns>
        Task<bool> TryRemove(T obj);
    }
}