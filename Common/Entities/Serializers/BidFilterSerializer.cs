using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;

namespace Lucent.Common.Entities.Serializers
{
    public class BidFilterSerializer : IEntitySerializer<BidFilter>
    {
        public BidFilter Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<BidFilter> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var filter = new BidFilter();

            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;
                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;
                }
            }

            return filter;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, BidFilter instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, BidFilter instance, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}