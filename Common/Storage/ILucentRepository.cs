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
        Task<ICollection<T>> Get();
        Task<T> Get(string key);
        Task<bool> TryInsert(T obj);
        Task<bool> TryUpdate(T obj);
        Task<bool> TryRemove(T obj);
    }
}