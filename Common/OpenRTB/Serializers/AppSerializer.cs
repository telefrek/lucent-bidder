using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AppEntitySerializer : IEntitySerializer<App>
    {
        /// <inheritdoc />
        public App Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<App> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new App();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.Name = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.Domain = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.AppCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 5:
                                instance.SectionCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 6:
                                instance.PageCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 7:
                                instance.Version = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 8:
                                instance.BundleId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 9:
                                instance.HasPrivacyPolicy = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 10:
                                instance.IsPaidVersion = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 11:
                                instance.Publisher = await serializationStreamReader.ReadAsAsync<Publisher>();
                                break;
                            case 12:
                                instance.Content = await serializationStreamReader.ReadAsAsync<Content>();
                                break;
                            case 13:
                                instance.Keywords = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 16:
                                instance.StoreUrl = await serializationStreamReader.ReadStringAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "name":
                        instance.Name = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "domain":
                        instance.Domain = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "cat":
                        instance.AppCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "sectioncat":
                        instance.SectionCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "pagecat":
                        instance.PageCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "ver":
                        instance.Version = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "bundle":
                        instance.BundleId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "privacypolicy":
                        instance.HasPrivacyPolicy = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "paid":
                        instance.IsPaidVersion = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "ublisher":
                        instance.Publisher = await serializationStreamReader.ReadAsAsync<Publisher>();
                        break;
                    case "content":
                        instance.Content = await serializationStreamReader.ReadAsAsync<Content>();
                        break;
                    case "keywords":
                        instance.Keywords = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "storeurl":
                        instance.StoreUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, App instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, App instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "name" }, instance.Name);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "domain" }, instance.Domain);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "cat" }, instance.AppCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "sectioncat" }, instance.SectionCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "pagecat" }, instance.PageCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "ver" }, instance.Version);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "bundle" }, instance.BundleId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "privacypolicy" }, instance.HasPrivacyPolicy);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "paid" }, instance.IsPaidVersion);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "publisher" }, instance.Publisher);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "content" }, instance.Content);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "keywords" }, instance.Keywords);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "storeurl" }, instance.StoreUrl);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}