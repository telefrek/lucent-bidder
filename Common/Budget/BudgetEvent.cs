using System;
using Lucent.Common.Entities;
using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetEvent
    {
        /// <summary>
        /// The id for the entity
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "id")]
        public string EntityId { get; set; }

        /// <summary>
        /// The type of entity
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "eType")]
        public EntityType EnttyType { get; set; }

        /// <summary>
        /// Flag to indicate if the entity budget is exhausted
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "exhausted")]
        public bool Exhausted { get; set; }
    }
}