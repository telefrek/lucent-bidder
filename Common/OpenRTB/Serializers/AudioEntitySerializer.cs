using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AudioEntitySerializer : IEntitySerializer<Audio>
    {
        /// <inheritdoc />
        public Audio Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<Audio> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Audio();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.MimeTypes = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 2:
                                instance.MinDuration = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 3:
                                instance.MaxDuration = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 4:
                                instance.Protocols = await serializationStreamReader.ReadAsArrayAsync<VideoProtocol>();
                                break;
                            case 5:
                                instance.Delay = await serializationStreamReader.ReadAsAsync<StartDelay>();
                                break;
                            case 6:
                                instance.Sequence = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 7:
                                instance.BlockedAttributes = await serializationStreamReader.ReadAsArrayAsync<BlockedCreative>();
                                break;
                            case 8:
                                instance.MaxExtended = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 9:
                                instance.MinBitrate = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 10:
                                instance.MaxBitrate = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 11:
                                instance.DeliveryMethods = await serializationStreamReader.ReadAsArrayAsync<ContentDeliveryMethod>();
                                break;
                            case 12:
                                instance.CompanionAds = await serializationStreamReader.ReadAsArrayAsync<Banner>();
                                break;
                            case 13:
                                instance.Frameworks = await serializationStreamReader.ReadAsArrayAsync<ApiFramework>();
                                break;
                            case 20:
                                instance.CompanionTypes = await serializationStreamReader.ReadAsArrayAsync<CompanionType>();
                                break;
                            case 21:
                                instance.MaxAds = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 22:
                                instance.AudioFeedType = await serializationStreamReader.ReadAsAsync<FeedType>();
                                break;
                            case 23:
                                instance.IsStitched = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 24:
                                instance.VolumeNormalization = await serializationStreamReader.ReadAsAsync<VolumeNormalizationMode>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "mimes":
                        instance.MimeTypes = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "minduration":
                        instance.MinDuration = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "maxduration":
                        instance.MaxDuration = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "protocols":
                        instance.Protocols = await serializationStreamReader.ReadAsArrayAsync<VideoProtocol>();
                        break;
                    case "startdelay":
                        instance.Delay = await serializationStreamReader.ReadAsAsync<StartDelay>();
                        break;
                    case "sequence":
                        instance.Sequence = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "battr":
                        instance.BlockedAttributes = await serializationStreamReader.ReadAsArrayAsync<BlockedCreative>();
                        break;
                    case "maxextended":
                        instance.MaxExtended = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "minbitrate":
                        instance.MinBitrate = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "maxbitrate":
                        instance.MaxBitrate = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "delivery":
                        instance.DeliveryMethods = await serializationStreamReader.ReadAsArrayAsync<ContentDeliveryMethod>();
                        break;
                    case "companionad":
                        instance.CompanionAds = await serializationStreamReader.ReadAsArrayAsync<Banner>();
                        break;
                    case "api":
                        instance.Frameworks = await serializationStreamReader.ReadAsArrayAsync<ApiFramework>();
                        break;
                    case "companiontype":
                        instance.CompanionTypes = await serializationStreamReader.ReadAsArrayAsync<CompanionType>();
                        break;
                    case "maxseq":
                        instance.MaxAds = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "feed":
                        instance.AudioFeedType = await serializationStreamReader.ReadAsAsync<FeedType>();
                        break;
                    case "stitched":
                        instance.IsStitched = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "nvol":
                        instance.VolumeNormalization = await serializationStreamReader.ReadAsAsync<VolumeNormalizationMode>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, Audio instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Audio instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "mimes" }, instance.MimeTypes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "minduration" }, instance.MinDuration);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "maxduration" }, instance.MaxDuration);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "protocols" }, instance.Protocols);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "startdelay" }, instance.Delay);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "sequence" }, instance.Sequence);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "battr" }, instance.BlockedAttributes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "maxextended" }, instance.MaxExtended);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "minbitrate" }, instance.MinBitrate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "maxbitrate" }, instance.MaxBitrate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "delivery" }, instance.DeliveryMethods);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "companionad" }, instance.CompanionAds);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "api" }, instance.Frameworks);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 20, Name = "companiontype" }, instance.CompanionTypes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 21, Name = "maxxeq" }, instance.MaxAds);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 22, Name = "feed" }, instance.AudioFeedType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 23, Name = "stitched" }, instance.IsStitched);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 24, Name = "nvol" }, instance.VolumeNormalization);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}