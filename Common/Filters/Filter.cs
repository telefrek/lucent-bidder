using System;

namespace Lucent.Common.Filters
{
    public class Filter
    {
        public string Property { get; set; }
        public object Value { get; set; }
        public object[] Values { get; set; }
        public FilterType FilterType { get; set; }
        public Type PropertyType { get; set; }
    }

                // Need a method to create filters in a generic way...
    public enum FilterType
    {
        EQ = 0,
        NEQ = 1,
        GT = 2,
        GTE = 3,
        LT = 4,
        LTE = 5,
        IN = 6,
        NOTIN = 7,
    }
}