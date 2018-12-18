using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Simple implementation without generics
    /// </summary>
    public class LucentMessage : IMessage
    {
        /// <inheritdoc />
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public MessageState State { get; set; }

        /// <inheritdoc />
        public string Body { get; set; }

        /// <inheritdoc />
        public long Timestamp { get; set; }

        /// <inheritdoc />
        public string CorrelationId { get; set; }

        /// <inheritdoc />
        public string ContentType { get; set; }

        /// <inheritdoc />
        public string Route { get; set; }

        /// <inheritdoc />
        public bool FirstDelivery { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public Task Load(byte[] buffer)
        {
            Body = Encoding.UTF8.GetString(buffer);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<byte[]> ToBytes() => Task.FromResult(Encoding.UTF8.GetBytes(Body ?? string.Empty));
    }

    /// <summary>
    /// Generic message using serializers to compress the contents
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LucentMessage<T> : IMessage
    where T : class, new()
    {
        ISerializationContext _serializationContext;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationContext">The current serialization context</param>
        public LucentMessage(ISerializationContext serializationContext) => _serializationContext = serializationContext;

        /// <inheritdoc />
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public MessageState State { get; set; }

        /// <inheritdoc />
        public T Body { get; set; }

        /// <inheritdoc />
        public long Timestamp { get; set; }

        /// <inheritdoc />
        public string CorrelationId { get; set; }

        /// <inheritdoc />
        public string ContentType { get; set; } = "application/json";

        /// <inheritdoc />
        public string Route { get; set; }

        /// <inheritdoc />
        public bool FirstDelivery { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public async Task Load(byte[] buffer)
        {
            Body = null;
            switch ((ContentType ?? "").ToLowerInvariant())
            {
                case "application/x-protobuf":
                    Body = await _serializationContext.ReadFrom<T>(new MemoryStream(buffer), false, SerializationFormat.PROTOBUF);
                    break;
                default:
                    Body = await _serializationContext.ReadFrom<T>(new MemoryStream(buffer), false, SerializationFormat.JSON);
                    break;
            }
        }

        /// <inheritdoc />
        public async Task<byte[]> ToBytes()
        {
            switch (ContentType.ToLowerInvariant())
            {
                case "application/x-protobuf":
                    using (var ms = new MemoryStream())
                    {
                        await _serializationContext.WriteTo(Body, ms, true, SerializationFormat.PROTOBUF);
                        return ms.ToArray();
                    }
                default:
                    using (var ms = new MemoryStream())
                    {
                        await _serializationContext.WriteTo(Body, ms, true, SerializationFormat.JSON);
                        return ms.ToArray();
                    }
            }
        }
    }
}