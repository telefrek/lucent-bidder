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
        public IStorageRepository<T, K> GetRepository<T, K>() where T : IStorageEntity<K>, new()
        {
            lock (_syncLock)
            {
                if (!_entities.ContainsKey(typeof(T)))
                    _entities.Add(typeof(T), new List<T>());

                return new InMemoryRepository<T, K>
                {
                    Entities = _entities[typeof(T)] as List<T>,
                };
            }
        }

        /// <inheritdoc />
        public void RegisterRepository<T, K>(IStorageRepository<T, K> repository) where T : IStorageEntity<K>, new()
        {
            // Ignore this for in memory
        }

        /// <inheritdoc />
        public class InMemoryRepository<T, K> : IStorageRepository<T, K> where T : IStorageEntity<K>
        {
            /// <summary>
            /// List of entities available
            /// </summary>
            /// <value></value>
            public List<T> Entities { get; set; }

            /// <inheritdoc />
            public Task<ICollection<T>> Get() => Task.FromResult((ICollection<T>)Entities);

            /// <inheritdoc />
            public Task<T> Get(K key) => Task.FromResult(Entities.FirstOrDefault(e => e.Id.Equals(key)));


            /// <inheritdoc />
            public Task<ICollection<T>> GetAll(K key) => Task.FromResult((ICollection<T>)Entities.Where(e => e.Id.Equals(key)).ToList());

            /// <inheritdoc />
            public Task<bool> TryInsert(T obj)
            {
                if (!Entities.Exists(e => e.Id.Equals(obj.Id)))
                {
                    Entities.Add(obj);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }

            /// <inheritdoc />
            public Task<bool> TryRemove(T obj) => Task.FromResult(Entities.Exists(e => e.Id.Equals(obj.Id) && e.ETag == obj.ETag) ? Entities.Remove(obj) : false);

            /// <inheritdoc />
            public async Task<bool> TryUpdate(T obj) => await TryRemove(obj) ? await TryInsert(obj) : false;
        }
    }
}