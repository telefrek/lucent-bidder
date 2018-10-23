using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;

namespace Lucent.Common.Entities.Serializers
{
    internal class CampaignSerializer : IEntitySerializer<Campaign>
    {
        public Campaign Read(ISerializationStreamReader serializationStreamReader) => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Campaign> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (!await serializationStreamReader.StartObjectAsync())
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
                            case 1:
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.Name = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.Spend = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 4:
                                instance.Schedule = await serializationStreamReader.ReadAsAsync<CampaignSchedule>();
                                break;
                            case 5:
                                instance.BidFilter = await serializationStreamReader.ReadAsAsync<BidFilter>();
                                if (instance.BidFilter != null)
                                    instance.IsFiltered = instance.BidFilter.GenerateCode();
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
                    case "schedule":
                        instance.Schedule = await serializationStreamReader.ReadAsAsync<CampaignSchedule>();
                        break;
                    case "filters":
                        instance.BidFilter = await serializationStreamReader.ReadAsAsync<BidFilter>();
                        if (instance.BidFilter != null)
                            instance.IsFiltered = instance.BidFilter.GenerateCode();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            
            if(!await serializationStreamReader.EndObjectAsync())
                return null;
                
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Campaign instance) => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Campaign instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "name" }, instance.Name);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "spend" }, instance.Spend);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "schedule" }, instance.Schedule);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "filters" }, instance.BidFilter);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}