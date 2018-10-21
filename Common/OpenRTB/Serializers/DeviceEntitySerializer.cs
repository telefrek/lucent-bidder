using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DeviceEntitySerializer : IEntitySerializer<Device>
    {
        /// <inheritdoc />
        public Device Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<Device> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Device();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.DoNotTrack = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 2:
                                instance.UserAgent = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.Ipv4 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.Geo = await serializationStreamReader.ReadAsAsync<Geo>();
                                break;
                            case 5:
                                instance.DeviceIdSHA1 = await serializationStreamReader.ReadStringAsync(); ;
                                break;
                            case 6:
                                instance.DeviceIdMD5 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 7:
                                instance.PlatformIdSHA1 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 8:
                                instance.PlatformIdMD5 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 9:
                                instance.Ipv6 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 10:
                                instance.Carrier = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 11:
                                instance.Language = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 12:
                                instance.Make = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 13:
                                instance.Model = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 14:
                                instance.OS = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 15:
                                instance.OSVersion = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 16:
                                instance.SupportJS = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 17:
                                instance.NetworkConnection = await serializationStreamReader.ReadAsAsync<ConnectionType>();
                                break;
                            case 18:
                                instance.DeviceType = await serializationStreamReader.ReadAsAsync<DeviceType>();
                                break;
                            case 19:
                                instance.FlashVersion = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 20:
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 21:
                                instance.MACSHA1 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 22:
                                instance.MACMD5 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 23:
                                instance.LimitedAdTracking = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 24:
                                instance.HardwareVersion = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 25:
                                instance.W = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 26:
                                instance.H = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 27:
                                instance.PPI = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 28:
                                instance.PixelRatio = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 29:
                                instance.SupportsGeoFetch = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 30:
                                instance.MobileCarrierCode = await serializationStreamReader.ReadStringAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;

                    case "dnt":
                        instance.DoNotTrack = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "ua":
                        instance.UserAgent = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "ip":
                        instance.Ipv4 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "geo":
                        instance.Geo = await serializationStreamReader.ReadAsAsync<Geo>();
                        break;
                    case "didsha1":
                        instance.DeviceIdSHA1 = await serializationStreamReader.ReadStringAsync(); ;
                        break;
                    case "didmd5":
                        instance.DeviceIdMD5 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "dpidsha1":
                        instance.PlatformIdSHA1 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "dpidmd5":
                        instance.PlatformIdMD5 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "ipv6":
                        instance.Ipv6 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "carrier":
                        instance.Carrier = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "language":
                        instance.Language = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "make":
                        instance.Make = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "model":
                        instance.Model = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "os":
                        instance.OS = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "osv":
                        instance.OSVersion = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "js":
                        instance.SupportJS = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "connectiontype":
                        instance.NetworkConnection = await serializationStreamReader.ReadAsAsync<ConnectionType>();
                        break;
                    case "devicetype":
                        instance.DeviceType = await serializationStreamReader.ReadAsAsync<DeviceType>();
                        break;
                    case "flashver":
                        instance.FlashVersion = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "ifa":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "macsha1":
                        instance.MACSHA1 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "macmd5":
                        instance.MACMD5 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "lmt":
                        instance.LimitedAdTracking = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "hwv":
                        instance.HardwareVersion = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "w":
                        instance.W = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "h":
                        instance.H = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "ppi":
                        instance.PPI = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "pxratio":
                        instance.PixelRatio = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "geofetch":
                        instance.SupportsGeoFetch = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "mccmnc":
                        instance.MobileCarrierCode = await serializationStreamReader.ReadStringAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, Device instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Device instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "dnt" }, instance.DoNotTrack);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "ua" }, instance.UserAgent);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "ip" }, instance.Ipv4);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "geo" }, instance.Geo);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "didsha1" }, instance.DeviceIdSHA1);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "didmd5" }, instance.DeviceIdMD5);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "dpidsha1" }, instance.PlatformIdSHA1);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "dpidmd5" }, instance.PlatformIdMD5);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "ipv6" }, instance.Ipv6);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "carrier" }, instance.Carrier);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "language" }, instance.Language);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "make" }, instance.Make);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "model" }, instance.Model);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "os" }, instance.OS);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "osv" }, instance.OSVersion);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "js" }, instance.SupportJS);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 17, Name = "connectiontype" }, instance.NetworkConnection);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 18, Name = "devicetype" }, instance.DeviceType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 19, Name = "flashver" }, instance.FlashVersion);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 20, Name = "ifa" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 21, Name = "macsha1" }, instance.MACSHA1);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 22, Name = "macmd5" }, instance.MACMD5);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 23, Name = "lmt" }, instance.LimitedAdTracking);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 24, Name = "hwv" }, instance.HardwareVersion);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 25, Name = "w" }, instance.W);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 26, Name = "h" }, instance.H);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 27, Name = "ppi" }, instance.PPI);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 28, Name = "pxratio" }, instance.PixelRatio);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 29, Name = "geofetch" }, instance.SupportsGeoFetch);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 30, Name = "mccmnc" }, instance.MobileCarrierCode);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}