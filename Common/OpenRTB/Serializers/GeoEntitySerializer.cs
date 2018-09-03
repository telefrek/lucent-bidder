using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class GeoEntitySerializer : IEntitySerializer<Geo>
    {
        public Geo Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Geo> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Geo();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.Latitude = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 2:
                                instance.Longitude = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 3:
                                instance.Country = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.Region = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 5:
                                instance.RegionFips = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 6:
                                instance.Metro = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 7:
                                instance.City = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 8:
                                instance.Zip = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 9:
                                instance.GeoType = await serializationStreamReader.ReadAsAsync<GeoType>();
                                break;
                            case 10:
                                instance.UtcOffset = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 11:
                                instance.Accuracy = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 12:
                                instance.LastFixed = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 13:
                                instance.ISP = await serializationStreamReader.ReadAsAsync<ISP>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "lat":
                        instance.Latitude = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "lon":
                        instance.Longitude = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "type":
                        instance.GeoType = await serializationStreamReader.ReadAsAsync<GeoType>();
                        break;
                    case "accuracy":
                        instance.Accuracy = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "lastfix":
                        instance.LastFixed = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "ipservice":
                        instance.ISP = await serializationStreamReader.ReadAsAsync<ISP>();
                        break;
                    case "country":
                        instance.Country = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "region":
                        instance.Region = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "regionfips104":
                        instance.RegionFips = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "metro":
                        instance.Metro = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "city":
                        instance.City = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "zip":
                        instance.Zip = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "utcoffset":
                        instance.UtcOffset = await serializationStreamReader.ReadIntAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Geo instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Geo instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "lat" }, instance.Latitude);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "lon" }, instance.Longitude);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "country" }, instance.Country);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "region" }, instance.Region);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "regionfips104" }, instance.RegionFips);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "metro" }, instance.Metro);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "city" }, instance.City);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "zip" }, instance.Zip);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "type" }, instance.GeoType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "utcoffset" }, instance.UtcOffset);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "accuracy" }, instance.Accuracy);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "lastfix" }, instance.LastFixed);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "ipservice" }, instance.ISP);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}