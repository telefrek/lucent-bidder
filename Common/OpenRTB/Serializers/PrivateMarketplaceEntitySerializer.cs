using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class PrivateMarketplaceEntitySerializer : IEntitySerializer<PrivateMarketplace>
    {
        public PrivateMarketplace Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<PrivateMarketplace> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new PrivateMarketplace();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.IsPrivate = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 2:
                                instance.Deals = await serializationStreamReader.ReadAsArrayAsync<Deal>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "private_auction":
                        instance.IsPrivate = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "deals":
                        instance.Deals = await serializationStreamReader.ReadAsArrayAsync<Deal>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, PrivateMarketplace instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, PrivateMarketplace instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "private_auction" }, instance.IsPrivate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "deals" }, instance.Deals);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}