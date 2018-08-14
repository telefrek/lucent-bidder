using System;
using System.Text;
using Lucent.Common.Serialization;

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

        public void Load(byte[] buffer) => Body = Encoding.UTF8.GetString(buffer);

        public byte[] ToBytes() => Encoding.UTF8.GetBytes(Body ?? string.Empty);
    }

    /// <summary>
    /// Generic message using serializers to compress the contents
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LucentMessage<T> : IMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public MessageState State { get; set; }
        public T Body { get; set; }

        public void Load(byte[] buffer)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ToBytes()
        {
            throw new System.NotImplementedException();
        }
    }
}