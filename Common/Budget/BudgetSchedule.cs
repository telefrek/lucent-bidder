using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Class to describe budget scheduling
    /// </summary>
    public class BudgetSchedule
    {
        /// <summary>
        /// The maximum amount per hour
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "hourly")]
        public double HourlyCap { get; set; }

        /// <summary>
        /// The maximum amount per day
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "daily")]
        public double DailyCap { get; set; }

        /// <summary>
        /// The type of schedule to follow
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "type")]
        public ScheduleType ScheduleType { get; set; }
    }

    /// <summary>
    /// Type of budget schedule
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Even (same amount every segment)
        /// </summary>
        Even = 1,
        /// <summary>
        /// Spend as soon as possible, up to the cap
        /// </summary>
        Aggressive = 2,
        /// <summary>
        /// Use a custom schedule (not supported yet)
        /// </summary>
        Custom = 3,
    }
}