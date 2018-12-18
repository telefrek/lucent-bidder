using System;
using Lucent.Common.Serialization;

namespace Lucent.Common.Events
{
    /// <summary>
    /// Base event
    /// </summary>
    public class LucentEvent
    {
        /// <summary>
        /// Event Id
        /// </summary>
        /// <returns></returns>
        [SerializationProperty(1, "id")]
        public Guid Id { get; set; } = SequentialGuid.NextGuid();

        /// <summary>
        /// Created time
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The type of event
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "type")]
        public EventType EventType { get; set; }
    }
}