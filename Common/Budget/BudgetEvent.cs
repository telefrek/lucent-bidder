using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetEvent
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
        public double Amount { get; set; }
    }
}