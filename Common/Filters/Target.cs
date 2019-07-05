using System;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Filters
{
    /// <summary>
    /// Custom Target object
    /// </summary>
    public class Target
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
        public FilterType TargetType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "value")]
        public TargetValue Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "values")]
        public TargetValue[] Values { get; set; }

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
    public class TargetValue
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
        public static implicit operator TargetValue(string s) => new TargetValue { SValue = s };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator string(TargetValue f) => f.SValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        public static implicit operator TargetValue(int i) => new TargetValue { IValue = i };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator int(TargetValue f) => f.IValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        public static implicit operator TargetValue(long l) => new TargetValue { LValue = l };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator long(TargetValue f) => f.LValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator TargetValue(bool b) => new TargetValue { BValue = b };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator bool(TargetValue f) => f.BValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator TargetValue(double d) => new TargetValue { DValue = d };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator double(TargetValue f) => f.DValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="log"></param>
        public static TargetValue Cast(object o, ILogger log)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                    return new TargetValue { IValue = (int)o };
                case TypeCode.Int64:
                    return new TargetValue { LValue = (long)o };
                case TypeCode.Double:
                    return new TargetValue { DValue = (double)o };
                case TypeCode.Boolean:
                    return new TargetValue { BValue = (bool)o };
                case TypeCode.String:
                    return new TargetValue { SValue = (string)o };
            }

            return null;
        }
    }
}