using System;
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
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "amt")]
        public double Amount { get; set; } = 1;

        /// <summary>
        /// Get/Set the correlation id
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "correlationId")]
        public Guid CorrelationId { get; set; }
    }
}