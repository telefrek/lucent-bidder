using System;
using System.Threading.Tasks;
using Lucent.Common.Entities.Events;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Watches for entity events
    /// </summary>
    public interface IEntityWatcher
    {
        /// <summary>
        /// Register a watccher for the given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="onUpdate"></param>
        void Register(string entityId, Func<EntityEvent, Task> onUpdate);
    }
}