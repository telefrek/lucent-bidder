using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class RegulationEntitySerializer : IEntitySerializer<Regulation>
    {
        public Regulation Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Regulation> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Regulation();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.ProtectsChildren = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "coppa":
                        instance.ProtectsChildren = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Regulation instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Regulation instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "coppa" }, instance.ProtectsChildren);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}