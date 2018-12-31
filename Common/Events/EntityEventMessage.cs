using Lucent.Common.Messaging;
using Lucent.Common.Serialization;

namespace Lucent.Common.Events
{
    /// <summary>
    /// Event specific message
    /// </summary>
    public class EntityEventMessage : LucentMessage<EntityEvent>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationContext"></param>
        /// <returns></returns>
        public EntityEventMessage(ISerializationContext serializationContext) : base(serializationContext)
        {
        }
    }
}