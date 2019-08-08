using System;

namespace Lucent.Common.Scheduling
{
    /// <summary>
    /// Represents a time table for tracking whether actions are allowed for a given hour
    /// </summary>
    public class SchedulingTable
    {
        readonly bool[] _state;
        readonly int _offset;

        /// <summary>
        /// Build a new table
        /// </summary>
        /// <param name="state">The state for each 24 hour slot in the schedule</param>
        /// <param name="current">The date time to consider</param>
        /// <param name="offset">The offset</param>
        public SchedulingTable(bool[] state, DateTime current, int offset)
        {
            _state = state;
            Current = current.ToUniversalTime();
            _offset = offset;
        }

        /// <summary>
        /// Clone the schedule
        /// </summary>
        /// <returns></returns>
        public SchedulingTable Clone() => new SchedulingTable(_state, Current, _offset);

        /// <summary>
        /// Check if the current time is available for scheduling
        /// </summary>
        public bool IsSchedulable => _state[Hour];

        /// <summary>
        /// Get the current offset
        /// </summary>
        public DateTime Current { get; set; }

        /// <summary>
        /// Get the current hour
        /// </summary>
        /// <value></value>
        public int Hour => Current.AddHours(-1 * _offset).Hour;

        /// <summary>
        /// Get the current date
        /// </summary>
        /// <returns></returns>
        public int Day => Current.AddHours(-1 * _offset).Day;

        /// <summary>
        /// Check if the schedule is advanced to the next day
        /// </summary>
        public bool IsNextDay => DateTime.UtcNow.AddHours(-1 * _offset).Day != Day;

        /// <summary>
        /// Check if the schedule is advanced to the next hour
        /// </summary>
        public bool IsNextHour => DateTime.UtcNow.AddHours(-1 * _offset).Hour != Hour;

        /// <summary>
        /// Advance the table to the next slot
        /// </summary>
        /// <returns>True if the table advanced</returns>
        public bool Advance()
        {
            if (IsNextHour || IsNextDay)
            {
                Current = DateTime.UtcNow;
                return true;
            }

            return false;
        }
    }
}