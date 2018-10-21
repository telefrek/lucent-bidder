using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucent.Common.Storage
{
    public class InMemoryStorage : IStorageManager
    {
        Dictionary<Type, object> _entities = new Dictionary<Type, object>();

        public ILucentRepository<T> GetRepository<T>() where T : IStorageEntity, new()
        {
            if (!_entities.ContainsKey(typeof(T)))
                _entities.Add(typeof(T), new List<T>());

            return new InMemoryRepository<T>
            {
                Entities = _entities[typeof(T)] as List<T>,
            };
        }
    }

    public class InMemoryRepository<T> : ILucentRepository<T> where T : IStorageEntity
    {
        public List<T> Entities { get; set; }

        public Task<ICollection<T>> Get() => Task.FromResult((ICollection<T>)Entities);

        public Task<T> Get(string key) => Task.FromResult(Entities.FirstOrDefault(e => e.Id.Equals(key)));

        public Task<bool> TryInsert(T obj)
        {
            if (!Entities.Contains(obj))
            {
                Entities.Add(obj);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> TryRemove(T obj)
        {
            return Task.FromResult(Entities.Remove(obj));
        }

        public Task<bool> TryUpdate(T obj)
        {
            if (Entities.Remove(obj))
            {
                Entities.Add(obj);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}