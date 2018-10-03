using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class SiteEntitySerializer : IEntitySerializer<Site>
    {
        public Site Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Site> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Site();
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
                                instance.SiteCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 5:
                                instance.SectionCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 6:
                                instance.PageCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 7:
                                instance.Page = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 8:
                                instance.IsPrivate = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 9:
                                instance.ReferrerUrl = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 10:
                                instance.SearchUrl = await serializationStreamReader.ReadStringAsync();
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
                            case 15:
                                instance.IsMobileOptimized = await serializationStreamReader.ReadBooleanAsync();
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
                        instance.SiteCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "sectioncat":
                        instance.SectionCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "pagecat":
                        instance.PageCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "page":
                        instance.Page = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "privacypolicy":
                        instance.IsPrivate = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "ref":
                        instance.ReferrerUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "search":
                        instance.SearchUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "publisher":
                        instance.Publisher = await serializationStreamReader.ReadAsAsync<Publisher>();
                        break;
                    case "content":
                        instance.Content = await serializationStreamReader.ReadAsAsync<Content>();
                        break;
                    case "keywords":
                        instance.Keywords = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "mobile":
                        instance.IsMobileOptimized = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Site instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Site instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "name" }, instance.Name);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "domain" }, instance.Domain);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "cat" }, instance.SiteCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "sectioncat" }, instance.SectionCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "pagecat" }, instance.PageCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "page" }, instance.Page);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "privacypolicy" }, instance.IsPrivate);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "ref" }, instance.ReferrerUrl);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "search" }, instance.SearchUrl);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "publisher" }, instance.Publisher);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "content" }, instance.Content);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "keywords" }, instance.Keywords);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "mobile" }, instance.IsMobileOptimized);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}