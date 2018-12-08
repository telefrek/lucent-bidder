using System.Reflection;

namespace Lucent.Common.Serialization._Internal
{
    internal class SerializableProperty

    {
        public SerializationPropertyAttribute Attribute { get; set; }
        public PropertyInfo Property { get; set; }
    }
}