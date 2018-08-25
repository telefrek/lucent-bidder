using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Representation of a generic data repository
    /// </summary>
    /// <typeparam name="T">The type of object managed by the repository</typeparam>
    public interface ILucentRepository<T, K>
    where T : new()
    {
        Task<ICollection<T>> Get();
        Task<T> Get(K key);
        Task<bool> TryInsert(T obj, Func<T,K> keyMap);
        Task<bool> TryUpdate(T obj, Func<T,K> keyMap);
        Task<bool> TryRemove(T obj, Func<T,K> keyMap);
    }
}