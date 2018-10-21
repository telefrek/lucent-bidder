using System;

namespace Lucent.Common.Filters
{
    /// <summary>
    /// Custom filter object
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public object Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public object[] Values { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public FilterType FilterType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Type PropertyType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FilterType
    {
        /// <value></value>
        EQ = 0,
        /// <value></value>
        NEQ = 1,
        /// <value></value>
        GT = 2,
        /// <value></value>
        GTE = 3,
        /// <value></value>
        LT = 4,
        /// <value></value>
        LTE = 5,
        /// <value></value>
        IN = 6,
        /// <value></value>
        NOTIN = 7,
    }
}