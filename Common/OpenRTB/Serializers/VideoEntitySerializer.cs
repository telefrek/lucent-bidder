using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class VideoEntitySerializer : IEntitySerializer<Video>
    {
        public Video Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Video> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Video();
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
                                instance.Linearity = await serializationStreamReader.ReadAsAsync<VideoLinearity>();
                                break;
                            case 3:
                                instance.MinDuration = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 4:
                                instance.MaxDuration = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 5:
                                instance.Protocol = await serializationStreamReader.ReadAsAsync<VideoProtocol>();
                                break;
                            case 6:
                                instance.W = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 7:
                                instance.H = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 8:
                                instance.Delay = await serializationStreamReader.ReadAsAsync<StartDelay>();
                                break;
                            case 9:
                                instance.Sequence = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 10:
                                instance.BlockedAttributes = await serializationStreamReader.ReadAsArrayAsync<BlockedCreative>();
                                break;
                            case 11:
                                instance.MaxExtended = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 12:
                                instance.MinBitrate = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 13:
                                instance.MaxBitrate = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 14:
                                instance.BoxingAllowed = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 15:
                                instance.PlaybackMethods = await serializationStreamReader.ReadAsArrayAsync<PlaybackMethod>();
                                break;
                            case 16:
                                instance.DeliveryMethods = await serializationStreamReader.ReadAsArrayAsync<ContentDeliveryMethod>();
                                break;
                            case 17:
                                instance.Position = await serializationStreamReader.ReadAsAsync<AdPosition>();
                                break;
                            case 18:
                                instance.CompanionAds = await serializationStreamReader.ReadAsArrayAsync<Banner>();
                                break;
                            case 19:
                                instance.Frameworks = await serializationStreamReader.ReadAsArrayAsync<ApiFramework>();
                                break;
                            case 20:
                                instance.CompanionTypes = await serializationStreamReader.ReadAsArrayAsync<CompanionType>();
                                break;
                            case 21:
                                instance.Protocols = await serializationStreamReader.ReadAsArrayAsync<VideoProtocol>();
                                break;
                            case 22:
                                instance.Companion21 = await serializationStreamReader.ReadAsAsync<CompanionAd>();
                                break;
                            case 23:
                                instance.IsSkippable = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 24:
                                instance.SkipMinDuration = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 25:
                                instance.SkipAfter = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 26:
                                instance.Placement = await serializationStreamReader.ReadAsAsync<VideoPlacement>();
                                break;
                            case 27:
                                instance.PlaybackEnd = await serializationStreamReader.ReadAsAsync<PlaybackCessation>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "mimes":
                        instance.MimeTypes = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "linearity":
                        instance.Linearity = await serializationStreamReader.ReadAsAsync<VideoLinearity>();
                        break;
                    case "minduration":
                        instance.MinDuration = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "maxduration":
                        instance.MaxDuration = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "protocol":
                        instance.Protocol = await serializationStreamReader.ReadAsAsync<VideoProtocol>();
                        break;
                    case "w":
                        instance.W = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "h":
                        instance.H = await serializationStreamReader.ReadIntAsync();
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
                    case "boxingallowed":
                        instance.BoxingAllowed = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "playbackmethod":
                        instance.PlaybackMethods = await serializationStreamReader.ReadAsArrayAsync<PlaybackMethod>();
                        break;
                    case "delivery":
                        instance.DeliveryMethods = await serializationStreamReader.ReadAsArrayAsync<ContentDeliveryMethod>();
                        break;
                    case "pos":
                        instance.Position = await serializationStreamReader.ReadAsAsync<AdPosition>();
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
                    case "protocols":
                        instance.Protocols = await serializationStreamReader.ReadAsArrayAsync<VideoProtocol>();
                        break;
                    case "companionad_21":
                        instance.Companion21 = await serializationStreamReader.ReadAsAsync<CompanionAd>();
                        break;
                    case "skip":
                        instance.IsSkippable = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "skipmin":
                        instance.SkipMinDuration = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "skipafter":
                        instance.SkipAfter = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "placement":
                        instance.Placement = await serializationStreamReader.ReadAsAsync<VideoPlacement>();
                        break;
                    case "playbackend":
                        instance.PlaybackEnd = await serializationStreamReader.ReadAsAsync<PlaybackCessation>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Video instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Video instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "mimes" }, instance.MimeTypes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "linearity" }, instance.Linearity);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "minduration" }, instance.MinDuration);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "maxduration" }, instance.MaxDuration);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "protocol" }, instance.Protocol);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "w" }, instance.W);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "h" }, instance.H);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "startdelay" }, instance.Delay);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "sequence" }, instance.Sequence);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "battr" }, instance.BlockedAttributes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "maxextended" }, instance.MaxExtended);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "minbitrate" }, instance.MinBitrate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "maxbitrate" }, instance.MaxBitrate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "boxingallowed" }, instance.BoxingAllowed);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "playbackmethod" }, instance.PlaybackMethods);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "delivery" }, instance.DeliveryMethods);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 17, Name = "pos" }, instance.Position);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 18, Name = "companionad" }, instance.CompanionAds);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 19, Name = "api" }, instance.Frameworks);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 20, Name = "companiontype" }, instance.CompanionTypes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 21, Name = "protocols" }, instance.Protocols);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 22, Name = "companionad_21" }, instance.Companion21);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 23, Name = "skip" }, instance.IsSkippable);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 24, Name = "skipmin" }, instance.SkipMinDuration);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 25, Name = "skipafter" }, instance.SkipAfter);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 26, Name = "placement" }, instance.Placement);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 27, Name = "playbackend" }, instance.PlaybackEnd);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}