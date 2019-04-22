using System;
using Lucent.Common.Entities;
using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "id")]
        public string EntityId { get; set; }

        /// <summary>
        /// Get/Set the correlation id
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "correlationId")]
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// The type of entity requested
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "eType")]
        public EntityType EntityType {get;set;}
    }
}