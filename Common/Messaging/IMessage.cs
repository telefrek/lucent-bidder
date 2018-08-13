namespace Lucent.Common.Messaging
{
    public interface IMessage
    {
        string MessageId { get; set; }

        byte[] ToBytes();
        void Load(byte[] buffer);
    }
}