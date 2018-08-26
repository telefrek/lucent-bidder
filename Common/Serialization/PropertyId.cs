namespace Lucent.Common.Serialization
{
    public class PropertyId
    {
        public string Name { get; set; } = string.Empty;
        public ulong Id { get; set; }

        public static implicit operator PropertyId(ulong value) => new PropertyId { Id = value };
        public static implicit operator PropertyId(string value) => new PropertyId { Name = value };
    }
}