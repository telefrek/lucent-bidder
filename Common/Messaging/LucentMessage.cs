using System;
using System.IO;
using System.Text;
using Lucent.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Simple implementation without generics
    /// </summary>
    public class LucentMessage : IMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public MessageState State { get; set; }
        public string Body { get; set; }
        public long Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string ContentType { get; set; }
        public string Route { get; set; }
        public bool FirstDelivery { get; set; }

        public void Load(byte[] buffer) => Body = Encoding.UTF8.GetString(buffer);

        public byte[] ToBytes() => Encoding.UTF8.GetBytes(Body ?? string.Empty);
    }

    /// <summary>
    /// Generic message using serializers to compress the contents
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LucentMessage<T> : IMessage
    where T : class, new()
    {
        IServiceProvider _provider;
        public LucentMessage(IServiceProvider provider) => _provider = provider;
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public MessageState State { get; set; }
        public T Body { get; set; }
        public long Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string ContentType { get; set; }
        public string Route { get; set; }
        public bool FirstDelivery { get; set; }

        public void Load(byte[] buffer)
        {
            switch (ContentType.ToLowerInvariant())
            {
                case "application/x-protobuf":
                    Body = _provider.GetRequiredService<ISerializationRegistry>().GetSerializer<T>().Read(new MemoryStream(buffer).WrapSerializer(_provider, SerializationFormat.PROTOBUF, false).Reader);
                    break;
                default:
                    Body = _provider.GetRequiredService<ISerializationRegistry>().GetSerializer<T>().Read(new MemoryStream(buffer).WrapSerializer(_provider, SerializationFormat.JSON, false).Reader);
                    break;
            }
        }

        public byte[] ToBytes()
        {
            switch (ContentType.ToLowerInvariant())
            {
                case "application/x-protobuf":
                    using(var ms = new MemoryStream())
                    {
                        _provider.GetRequiredService<ISerializationRegistry>().GetSerializer<T>().Write(ms.WrapSerializer(_provider, SerializationFormat.PROTOBUF, true).Writer, Body);
                        return ms.ToArray();
                    }
                default:
                    using(var ms = new MemoryStream())
                    {
                        _provider.GetRequiredService<ISerializationRegistry>().GetSerializer<T>().Write(ms.WrapSerializer(_provider, SerializationFormat.JSON, true).Writer, Body);
                        return ms.ToArray();
                    }
            }
        }
    }
}