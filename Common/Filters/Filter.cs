using System;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;

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
        /// <value></value>
        [SerializationProperty(3, "bval")]
        public bool BValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "dval")]
        public double DValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "lval")]
        public long LValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator FilterValue(string s) => new FilterValue { SValue = s };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator string(FilterValue f) => f.SValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        public static implicit operator FilterValue(int i) => new FilterValue { IValue = i };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator int(FilterValue f) => f.IValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        public static implicit operator FilterValue(long l) => new FilterValue { LValue = l };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator long(FilterValue f) => f.LValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator FilterValue(bool b) => new FilterValue { BValue = b };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator bool(FilterValue f) => f.BValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator FilterValue(double d) => new FilterValue { DValue = d };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator double(FilterValue f) => f.DValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="log"></param>
        public static FilterValue Cast(object o, ILogger log)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                    return new FilterValue { IValue = (int)o };
                case TypeCode.Int64:
                    return new FilterValue { LValue = (long)o };
                case TypeCode.Double:
                    return new FilterValue { DValue = (double)o };
                case TypeCode.Boolean:
                    return new FilterValue { BValue = (bool)o };
                case TypeCode.String:
                    return new FilterValue { SValue = (string)o };
            }

            return null;
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
        /// <value></value>
        [Display(Name = "has value")]
        HASVALUE = 8,
    }
}