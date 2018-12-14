using System;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Serialization;

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
        [SerializationProperty(1, "property")]
        public string Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "value")]
        public FilterValue Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "values")]
        public FilterValue[] Values { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Type PropertyType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "ptype")]
        public string PTypeStr
        {
            get
            {
                return PropertyType.AssemblyQualifiedName;
            }
            set
            {
                PropertyType = Type.GetType(value, true, true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FilterValue
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "ival")]
        public int IValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "sval")]
        public string SValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator FilterValue(string s)
        {
            return new FilterValue { SValue = s };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator string(FilterValue f)
        {
            return f.SValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        public static implicit operator FilterValue(int i)
        {
            return new FilterValue { IValue = i };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator int(FilterValue f)
        {
            return f.IValue;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FilterType
    {
        /// <value></value>
        [Display(Name = "=")]
        EQ = 0,
        /// <value></value>
        [Display(Name = "<>")]
        NEQ = 1,
        /// <value></value>
        [Display(Name = ">")]
        GT = 2,
        /// <value></value>
        [Display(Name = ">=")]
        GTE = 3,
        /// <value></value>
        [Display(Name = "<")]
        LT = 4,
        /// <value></value>
        [Display(Name = "<=")]
        LTE = 5,
        /// <value></value>
        [Display(Name = "in")]
        IN = 6,
        /// <value></value>
        [Display(Name = "not in")]
        NOTIN = 7,
    }
}