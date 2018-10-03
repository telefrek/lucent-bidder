using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class FormatEntitySerializer : IEntitySerializer<Format>
    {
        public Format Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Format> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Format();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.W = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 2:
                                instance.H = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 3:
                                instance.WRatio = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 4:
                                instance.HRatio = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 5:
                                instance.WMin = await serializationStreamReader.ReadIntAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "w":
                        instance.W = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "h":
                        instance.H = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "wratio":
                        instance.WRatio = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "hratio":
                        instance.HRatio = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "wmin":
                        instance.WMin = await serializationStreamReader.ReadIntAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Format instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Format instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "w" }, instance.W);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "h" }, instance.H);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "wratio" }, instance.WRatio);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "hratio" }, instance.HRatio);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "wmin" }, instance.WMin);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}