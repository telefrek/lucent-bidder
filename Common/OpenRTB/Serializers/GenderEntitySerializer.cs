using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class GenderEntitySerializer : IEntitySerializer<Gender>
    {
        public Gender Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Gender> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            switch (await serializationStreamReader.ReadStringAsync())
            {
                case "M": return Gender.Male;
                case "F": return Gender.Female;
                default: return Gender.Other;
            }
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Gender instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Gender instance, CancellationToken token)
        {
            switch (instance)
            {
                case Gender.Male:
                    await serializationStreamWriter.WriteAsync("M");
                    break;
                case Gender.Female:
                    await serializationStreamWriter.WriteAsync("F");
                    break;
                default:
                    await serializationStreamWriter.WriteAsync("O");
                    break;
            }
        }
    }
}