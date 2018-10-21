using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// In memory storage provider
    /// </summary>
    public class InMemoryStorage : IStorageManager
    {
        Dictionary<Type, object> _entities = new Dictionary<Type, object>();
        object _syncLock = new object();

        /// <inheritdoc />
        public ILucentRepository<T> GetRepository<T>() where T : IStorageEntity, new()
        {
            lock (_syncLock)
            {
                if (!_entities.ContainsKey(typeof(T)))
                    _entities.Add(typeof(T), new List<T>());

                return new InMemoryRepository<T>
                {
                    Entities = _entities[typeof(T)] as List<T>,
                };
            }
        }

        /// <summary>
        /// In memory repository
        /// </summary>
        /// <typeparam name="T">The type of object to store</typeparam>
        public class InMemoryRepository<T> : ILucentRepository<T> where T : IStorageEntity
        {
            /// <summary>
            /// List of entities available
            /// </summary>
            /// <value></value>
            public List<T> Entities { get; set; }

            /// <inheritdoc />
            public Task<ICollection<T>> Get() => Task.FromResult((ICollection<T>)Entities);

            /// <inheritdoc />
            public Task<T> Get(string key) => Task.FromResult(Entities.FirstOrDefault(e => e.Id.Equals(key)));

            /// <inheritdoc />
            public Task<bool> TryInsert(T obj)
            {
                if (!Entities.Exists(e => e.Id == obj.Id))
                {
                    Entities.Add(obj);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }

            /// <inheritdoc />
            public Task<bool> TryRemove(T obj) => Task.FromResult(Entities.Exists(e => e.Id == obj.Id && e.ETag == obj.ETag) ? Entities.Remove(obj) : false);

            /// <inheritdoc />
            public async Task<bool> TryUpdate(T obj) => await TryRemove(obj) ? await TryInsert(obj) : false;
        }
    }
}