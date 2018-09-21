using System.IO;

namespace Lucent.Common.Storage
{
    public interface IFileBlob
    {
        string ContentType { get; set; }
        Stream Contents { get; set; }
        string Name { get; set; }
        string ETag { get; set; }
        string Id { get; set; }
    }
}