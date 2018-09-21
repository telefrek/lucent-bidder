using System.Threading.Tasks;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Represents a generic file storage system
    /// </summary>
    public interface IFileStore
    {
        Task<IFileBlob> GetFile(string id);
        Task<bool> TryCreateFile(IFileBlob blob);
        Task<bool> TryUpdateFile(IFileBlob blob);
    }
}