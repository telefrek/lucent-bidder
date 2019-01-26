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
        public decimal Amount { get; set; } = 1m;
    }
}