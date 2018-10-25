using System;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name="=")]
        EQ = 0,
        /// <value></value>
        [Display(Name="<>")]
        NEQ = 1,
        /// <value></value>
        [Display(Name=">")]
        GT = 2,
        /// <value></value>
        [Display(Name=">=")]
        GTE = 3,
        /// <value></value>
        [Display(Name="<")]
        LT = 4,
        /// <value></value>
        [Display(Name="<=")]
        LTE = 5,
        /// <value></value>
        [Display(Name="in")]
        IN = 6,
        /// <value></value>
        [Display(Name="not in")]
        NOTIN = 7,
    }
}