using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Filters;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;

namespace Lucent.Common.Entities.Serializers
{
    internal class CreativeContentSerializer : IEntitySerializer<CreativeContent>
    {
        public CreativeContent Read(ISerializationStreamReader serializationStreamReader) => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<CreativeContent> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new CreativeContent();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.ContentLocation = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.PreserveAspect = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 3:
                                instance.CanScale = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 4:
                                instance.BitRate = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 5:
                                instance.W = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 6:
                                instance.H = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 7:
                                instance.MimeType = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 8:
                                instance.Codec = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 9:
                                instance.Duration = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 10:
                                instance.Offset = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 11:
                                instance.CreativeUri = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 12:
                                instance.RawUri = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 13:
                                instance.ContentType = await serializationStreamReader.ReadAsAsync<ContentType>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "content_location":
                        instance.ContentLocation = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "preserve_aspect":
                        instance.PreserveAspect = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "can_scale":
                        instance.CanScale = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "bitrate":
                        instance.BitRate = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "w":
                        instance.W = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "h":
                        instance.H = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "mime_type":
                        instance.MimeType = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "codecc":
                        instance.Codec = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "duration":
                        instance.Duration = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "offset":
                        instance.Offset = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "creative_uri":
                        instance.CreativeUri = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "raw_uri":
                        instance.RawUri = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "content_type":
                        instance.ContentType = await serializationStreamReader.ReadAsAsync<ContentType>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }

            instance.HydrateFilter();
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, CreativeContent instance) => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, CreativeContent instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "content_location" }, instance.ContentLocation);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "preserve_aspect" }, instance.PreserveAspect);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "can_scale" }, instance.CanScale);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "bitrate" }, instance.BitRate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "w" }, instance.W);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "h" }, instance.H);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "mime_type" }, instance.MimeType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "codec" }, instance.Codec);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "duration" }, instance.Duration);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "offset" }, instance.Offset);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "creative_uri" }, instance.CreativeUri);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "raw_uri" }, instance.ContentLocation);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "content_type" }, instance.ContentType);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}