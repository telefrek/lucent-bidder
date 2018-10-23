using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;

namespace Lucent.Common.Entities.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public class BidFilterSerializer : IEntitySerializer<BidFilter>
    {
        /// <inheritdoc />
        public BidFilter Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<BidFilter> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (!await serializationStreamReader.StartObjectAsync())
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
                            case 1:
                                filter.AppFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                                break;
                            case 2:
                                filter.DeviceFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                                break;
                            case 3:
                                filter.GeoFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                                break;
                            case 4:
                                filter.ImpressionFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                                break;
                            case 5:
                                filter.SiteFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                                break;
                            case 6:
                                filter.UserFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;

                    case "app":
                        filter.AppFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                        break;
                    case "device":
                        filter.DeviceFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                        break;
                    case "geo":
                        filter.GeoFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                        break;
                    case "impression":
                        filter.ImpressionFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                        break;
                    case "site":
                        filter.SiteFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                        break;
                    case "user":
                        filter.UserFilters = await serializationStreamReader.ReadAsArrayAsync<Filter>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;
                }
            }

            return filter;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, BidFilter instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, BidFilter instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "app" }, instance.AppFilters);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "device" }, instance.DeviceFilters);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "geo" }, instance.GeoFilters);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "impression" }, instance.ImpressionFilters);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "site" }, instance.SiteFilters);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "user" }, instance.UserFilters);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}