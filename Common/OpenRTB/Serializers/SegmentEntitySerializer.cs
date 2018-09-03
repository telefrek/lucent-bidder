using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class SegmentEntitySerializer : IEntitySerializer<Segment>
    {
        public Segment Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Segment> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Segment();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.Name = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.Value = await serializationStreamReader.ReadStringAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "name":
                        instance.Name = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "value":
                        instance.Value = await serializationStreamReader.ReadStringAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Segment instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Segment instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "name" }, instance.Name);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "value" }, instance.Value);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}