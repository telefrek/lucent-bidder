using Lucent.Common.Messaging;
using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="BudgetEvent"></typeparam>
    public class BudgetEventMessage : LucentMessage<BudgetEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializationContext"></param>
        /// <returns></returns>
        public BudgetEventMessage(ISerializationContext serializationContext) : base(serializationContext)
        {
        }
    }
}