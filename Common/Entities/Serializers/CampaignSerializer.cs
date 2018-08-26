using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;

namespace Lucent.Common.Entities.Serializers
{
    internal class CampaignSerializer : IEntitySerializer<Campaign>
    {
        public Campaign Read(ISerializationStreamReader serializationStreamReader)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!serializationStreamReader.HasNext())
                    return null;

            var instance = new Campaign();
            while (serializationStreamReader.HasMoreProperties())
            {
                var propId = serializationStreamReader.Id;
                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 0:
                                instance.Id = serializationStreamReader.ReadString();
                                break;
                            case 1:
                                instance.Name = serializationStreamReader.ReadString();
                                break;
                            case 2:
                                instance.Spend = serializationStreamReader.ReadDouble();
                                break;
                            default:
                                serializationStreamReader.Skip();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = serializationStreamReader.ReadString();
                        break;
                    case "name":
                        instance.Name = serializationStreamReader.ReadString();
                        break;
                    case "spend":
                        instance.Spend = serializationStreamReader.ReadDouble();
                        break;
                    default:
                        serializationStreamReader.Skip();
                        break;

                }
            }
            return instance;
        }

        public async Task<Campaign> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Campaign();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 0:
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 1:
                                instance.Name = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.Spend = await serializationStreamReader.ReadDoubleAsync();
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
                    case "spend":
                        instance.Spend = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Campaign instance)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                serializationStreamWriter.Write(new PropertyId { Id = 0, Name = "id" }, instance.Id);
                serializationStreamWriter.Write(new PropertyId { Id = 1, Name = "name" }, instance.Name);
                serializationStreamWriter.Write(new PropertyId { Id = 2, Name = "spend" }, instance.Spend);
                serializationStreamWriter.EndObject();
                serializationStreamWriter.Flush();
            }
        }

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Campaign instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 0, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "name" }, instance.Name);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "spend" }, instance.Spend);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}