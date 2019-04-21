using Lucent.Common.Serialization;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Represents an action for a postback event
    /// </summary>
    public class PostbackAction
    {
        /// <summary>
        /// The name of the action
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "name")]
        public string Name { get; set; }

        /// <summary>
        /// The payout (if any) associated with the action
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "payout")]
        public double Payout {get; set;}
    }
}