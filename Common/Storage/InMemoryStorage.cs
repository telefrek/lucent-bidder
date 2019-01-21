using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// In memory storage provider
    /// </summary>
    public class InMemoryStorage : IStorageManager
    {
        static Dictionary<Type, object> _entities = new Dictionary<Type, object>();
        static object _syncLock = new object();
        IServiceProvider _provider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public InMemoryStorage(IServiceProvider provider) => _provider = provider;

        /// <inheritdoc />
        public IStorageRepository<T, K> GetRepository<T, K>()
            where T : IStorageEntity<K>, new()
        {
            lock (_syncLock)
            {
                if (!_entities.ContainsKey(typeof(T)))
                    _entities.Add(typeof(T), new List<T>());

                if (typeof(string).IsAssignableFrom(typeof(K)))
                {
                    dynamic obj = Activator.CreateInstance(typeof(BasicInMemoryRepository<>).MakeGenericType(typeof(T)));
                    obj.Entities = _entities[typeof(T)] as List<T>;
                    return obj as IStorageRepository<T, K>;
                }

                return new InMemoryRepository<T, K>(_provider)
                {
                    Entities = _entities[typeof(T)] as List<T>,
                };
            }
        }

        /// <inheritdoc />
        public void RegisterRepository<R, T, K>()
            where R : IStorageRepository<T, K>
            where T : IStorageEntity<K>, new()
        {
            // Ignore this for in memory
        }

        /// <inheritdoc />
        public class InMemoryRepository<T, K> : IStorageRepository<T, K> where T : IStorageEntity<K>
        {
            IServiceProvider _provider;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="provider"></param>
            public InMemoryRepository(IServiceProvider provider) => _provider = provider;

            /// <summary>
            /// List of entities available
            /// </summary>
            /// <value></value>
            public List<T> Entities { get; set; }

            /// <inheritdoc />
            public Task<ICollection<T>> GetAll() => Task.FromResult((ICollection<T>)Entities);

            /// <inheritdoc />
            public async Task<T> Get(K key)
            {
                var instance = Entities.FirstOrDefault(e => e.Id.Equals(key));
                if (typeof(Exchange).IsAssignableFrom(typeof(T)))
                {
                    var exchange = (Exchange)(object)instance;
                    if (exchange.Code != null)
                    {
                        await exchange.LoadExchange(_provider, exchange.Code.ToArray());
                        exchange.Code = null;
                    }
                }

                return instance;
            }


            /// <inheritdoc />
            public Task<ICollection<T>> GetAny(K key) => Task.FromResult((ICollection<T>)Entities.Where(e => e.Id.Equals(key)).ToList());

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
            public Task<bool> TryRemove(T obj)
            {
                var test = Entities.FirstOrDefault(e => e.Id.Equals(obj.Id) && e.ETag == obj.ETag);
                return Task.FromResult(test != null ? Entities.Remove(test) : false);
            }

            /// <inheritdoc />
            public async Task<bool> TryUpdate(T obj) => await TryRemove(obj) ? await TryInsert(obj) : false;
        }

        /// <inheritdoc />
        public class BasicInMemoryRepository<T> : IBasicStorageRepository<T> where T : IStorageEntity<string>, new()
        {
            /// <summary>
            /// List of entities available
            /// </summary>
            /// <value></value>
            public List<T> Entities { get; set; }

            /// <inheritdoc />
            public Task<ICollection<T>> GetAll() => Task.FromResult((ICollection<T>)Entities);

            /// <inheritdoc />
            public Task<T> Get(string key) => Task.FromResult(Entities.FirstOrDefault(e => e.Id.Equals(key)));


            /// <inheritdoc />
            public Task<ICollection<T>> GetAny(string key) => Task.FromResult((ICollection<T>)Entities.Where(e => e.Id.Equals(key)).ToList());

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
            public Task<bool> TryRemove(T obj)
            {
                var test = Entities.FirstOrDefault(e => e.Id.Equals(obj.Id) && e.ETag == obj.ETag);
                return Task.FromResult(test != null ? Entities.Remove(test) : false);
            }

            /// <inheritdoc />
            public async Task<bool> TryUpdate(T obj) => await TryRemove(obj) ? await TryInsert(obj) : false;
        }
    }
}