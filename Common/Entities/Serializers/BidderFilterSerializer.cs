using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Filters;
using Lucent.Common.Serialization;

namespace Lucent.Common.Entities.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public class BidderFilterSerializer : IEntitySerializer<BidderFilter>
    {

        /// <inheritdoc />
        public BidderFilter Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<BidderFilter> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (!await serializationStreamReader.StartObjectAsync())
                return null;

            var filter = new BidderFilter();

            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;
                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                filter.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                filter.BidFilter = await serializationStreamReader.ReadAsAsync<BidFilter>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        filter.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "bidfilter":
                        filter.BidFilter = await serializationStreamReader.ReadAsAsync<BidFilter>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;
                }
            }

            return filter;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, BidderFilter instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, BidderFilter instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "bidfilter" }, instance.BidFilter);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}