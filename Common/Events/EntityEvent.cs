using Lucent.Common.Entities;
using Lucent.Common.Serialization;

namespace Lucent.Common.Events
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityEvent : LucentEvent
    {
        /// <summary>
        /// The entity Id
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "entityId")]
        public string EntityId { get; set; }

        /// <summary>
        /// The type of entity for this update
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "entityType")]
        public EntityType EntityType { get; set; }
    }
}