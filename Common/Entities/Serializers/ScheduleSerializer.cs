using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;

namespace Lucent.Common.Entities.Serializers
{
    internal class CampaignScheduleSerializer : IEntitySerializer<CampaignSchedule>
    {
        public CampaignSchedule Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<CampaignSchedule> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new CampaignSchedule();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.StartDate = (await serializationStreamReader.ReadDateTimeAsync());
                                break;
                            case 2:
                                instance.EndDate = (await serializationStreamReader.ReadDateTimeAsync());
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "start":
                        instance.StartDate = (await serializationStreamReader.ReadDateTimeAsync());
                        break;
                    case "end":
                        instance.EndDate = (await serializationStreamReader.ReadDateTimeAsync());
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

        public void Write(ISerializationStreamWriter serializationStreamWriter, CampaignSchedule instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, CampaignSchedule instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "start" }, instance.StartDate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "end" }, instance.EndDate);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}