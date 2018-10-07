using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;

namespace Lucent.Common.Entities.Serializers
{
    internal class CreativeSearializer : IEntitySerializer<Creative>
    {
        public Creative Read(ISerializationStreamReader serializationStreamReader) => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Creative> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Creative();
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
                                instance.Title = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.Description = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 5:
                                instance.Contents.AddRange(await serializationStreamReader.ReadAsArrayAsync<CreativeContent>());
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
                    case "title":
                        instance.Title = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "desc":
                        instance.Description = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "contents":
                        instance.Contents.AddRange(await serializationStreamReader.ReadAsArrayAsync<CreativeContent>());
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Creative instance) => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Creative instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "name" }, instance.Name);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "title" }, instance.Title);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "desc" }, instance.Description);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "contents" }, instance.Contents.ToArray());
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}