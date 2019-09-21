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
        /// Modifier for the CPM value
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "modifier")]
        public double Modifier { get; set; }

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
}