using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;

namespace Lucent.Common.Entities.Serializers
{
    internal class ScheduleSerializer : IEntitySerializer<Schedule>
    {
        public Schedule Read(ISerializationStreamReader serializationStreamReader)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!serializationStreamReader.HasNext())
                    return null;

            try
            {
                var instance = new Schedule();
                while (serializationStreamReader.HasMoreProperties())
                {
                    var propId = serializationStreamReader.Id;
                    switch (propId.Name)
                    {
                        case "":
                            switch (propId.Id)
                            {
                                case 0:
                                    instance.StartDate = serializationStreamReader.ReadDateTime();
                                    break;
                                case 1:
                                    instance.EndDate = serializationStreamReader.ReadDateTime();
                                    break;
                                default:
                                    serializationStreamReader.Skip();
                                    break;
                            }
                            break;
                        case "start":
                            instance.StartDate = serializationStreamReader.ReadDateTime();
                            break;
                        case "end":
                            instance.EndDate = serializationStreamReader.ReadDateTime();
                            break;
                        default:
                            serializationStreamReader.Skip();
                            break;

                    }
                }
                return instance;
            }
            catch
            {

            }

            return null;
        }

        public async Task<Schedule> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Schedule();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 0:
                                instance.StartDate = (await serializationStreamReader.ReadDateTimeAsync());
                                break;
                            case 1:
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
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Schedule instance)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                serializationStreamWriter.Write(new PropertyId { Id = 0, Name = "start" }, instance.StartDate);
                serializationStreamWriter.Write(new PropertyId { Id = 1, Name = "end" }, instance.EndDate);
                serializationStreamWriter.EndObject();
                serializationStreamWriter.Flush();
            }
        }

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Schedule instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 0, Name = "start" }, instance.StartDate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "end" }, instance.EndDate);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}