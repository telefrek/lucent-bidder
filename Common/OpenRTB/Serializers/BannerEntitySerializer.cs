using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BannerEntitySerializer : IEntitySerializer<Banner>
    {
        /// <inheritdoc />
        public Banner Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<Banner> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Banner();
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
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.Position = await serializationStreamReader.ReadAsAsync<AdPosition>();
                                break;
                            case 5:
                                instance.BlockedTypes = await serializationStreamReader.ReadAsArrayAsync<BlockedType>();
                                break;
                            case 6:
                                instance.BlockedCreative = await serializationStreamReader.ReadAsArrayAsync<BlockedCreative>();
                                break;
                            case 7:
                                instance.MimeTypes = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 8:
                                instance.IsIFrame = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 9:
                                instance.ExpandableDirections = await serializationStreamReader.ReadAsArrayAsync<ExpandableDirection>();
                                break;
                            case 10:
                                instance.SupportedApi = await serializationStreamReader.ReadAsArrayAsync<ApiFramework>();
                                break;
                            case 11:
                                instance.WMax = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 12:
                                instance.HMax = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 13:
                                instance.WMin = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 14:
                                instance.HMin = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 15:
                                instance.Formats = await serializationStreamReader.ReadAsArrayAsync<Format>();
                                break;
                            case 16:
                                instance.IsBannerConcurrent = await serializationStreamReader.ReadBooleanAsync();
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
                            case "id":
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case "pos":
                                instance.Position = await serializationStreamReader.ReadAsAsync<AdPosition>();
                                break;
                            case "btype":
                                instance.BlockedTypes = await serializationStreamReader.ReadAsArrayAsync<BlockedType>();
                                break;
                            case "battr":
                                instance.BlockedCreative = await serializationStreamReader.ReadAsArrayAsync<BlockedCreative>();
                                break;
                            case "mimes":
                                instance.MimeTypes = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case "topframe":
                                instance.IsIFrame = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case "expdir":
                                instance.ExpandableDirections = await serializationStreamReader.ReadAsArrayAsync<ExpandableDirection>();
                                break;
                            case "api":
                                instance.SupportedApi = await serializationStreamReader.ReadAsArrayAsync<ApiFramework>();
                                break;
                            case "wmax":
                                instance.WMax = await serializationStreamReader.ReadIntAsync();
                                break;
                            case "hmax":
                                instance.HMax = await serializationStreamReader.ReadIntAsync();
                                break;
                            case "wmin":
                                instance.WMin = await serializationStreamReader.ReadIntAsync();
                                break;
                            case "hmin":
                                instance.HMin = await serializationStreamReader.ReadIntAsync();
                                break;
                            case "format":
                                instance.Formats = await serializationStreamReader.ReadAsArrayAsync<Format>();
                                break;
                            case "vcm":
                                instance.IsBannerConcurrent = await serializationStreamReader.ReadBooleanAsync();
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

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, Banner instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Banner instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "w" }, instance.W);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "h" }, instance.H);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "pos" }, instance.Position);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "btype" }, instance.BlockedTypes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "battr" }, instance.BlockedCreative);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "mimes" }, instance.MimeTypes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "topframe" }, instance.IsIFrame);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "expdir" }, instance.ExpandableDirections);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "api" }, instance.SupportedApi);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "wmax" }, instance.WMax);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "hmax" }, instance.HMax);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "wmin" }, instance.WMin);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "hmin" }, instance.HMin);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "format" }, instance.Formats);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "vcm" }, instance.IsBannerConcurrent);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}