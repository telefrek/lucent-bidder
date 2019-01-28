using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Microsoft.Extensions.Logging;

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
        ILogger<InMemoryStorage> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        public InMemoryStorage(IServiceProvider provider, ILogger<InMemoryStorage> logger)
        {
            _provider = provider;
            _log = logger;
        }

        /// <inheritdoc />
        public IStorageRepository<T> GetRepository<T>()
            where T : IStorageEntity, new()
        {
            lock (_syncLock)
            {
                if (!_entities.ContainsKey(typeof(T)))
                    _entities.Add(typeof(T), new List<T>());

                return new InMemoryRepository<T>(_provider, _log)
                {
                    Entities = _entities[typeof(T)] as List<T>,
                };
            }
        }

        /// <inheritdoc />
        public void RegisterRepository<R, T>()
            where R : IStorageRepository<T>
            where T : IStorageEntity, new()
        {
            // Ignore this for in memory
        }

        /// <inheritdoc />
        public class InMemoryRepository<T> : IStorageRepository<T> where T : IStorageEntity
        {
            IServiceProvider _provider;
            ILogger _log;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="provider"></param>
            /// <param name="log"></param>
            public InMemoryRepository(IServiceProvider provider, ILogger log)
            {
                _provider = provider;
                _log = log;
            }

            /// <summary>
            /// List of entities available
            /// </summary>
            /// <value></value>
            public List<T> Entities { get; set; }

            /// <inheritdoc />
            public Task<ICollection<T>> GetAll() => Task.FromResult((ICollection<T>)Entities);

            /// <inheritdoc />
            public async Task<T> Get(StorageKey key)
            {
                _log.LogInformation("Getting {0} ({1})", key, typeof(T).Name);
                var instance = Entities.FirstOrDefault(e => e.Key.Equals(key));
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
            public Task<ICollection<T>> GetAny(StorageKey key) => Task.FromResult((ICollection<T>)Entities.Where(e => e.Key.Equals(key)).ToList());

            /// <inheritdoc />
            public Task<bool> TryInsert(T obj)
            {
                _log.LogInformation("Inserting {0} ({1})", obj.Key, typeof(T).Name);
                if (!Entities.Exists(e => e.Key.Equals(obj.Key)))
                {
                    Entities.Add(obj);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }

            /// <inheritdoc />
            public Task<bool> TryRemove(T obj)
            {
                _log.LogInformation("Removing {0} ({1})", obj.Key, typeof(T).Name);
                var test = Entities.FirstOrDefault(e => e.Key.Equals(obj.Key) && e.ETag == obj.ETag);
                return Task.FromResult(test != null ? Entities.Remove(test) : false);
            }

            /// <inheritdoc />
            public async Task<bool> TryUpdate(T obj)
            {
                _log.LogInformation("Updating {0} ({1})", obj.Key, typeof(T).Name);
                return await TryRemove(obj) ? await TryInsert(obj) : false;
            }
        }
    }
}