using Lucent.Common.Entities;

namespace Lucent.Common.Media
{
    /// <summary>
    /// Scans media information
    /// </summary>
    public interface IMediaScanner
    {
        /// <summary>
        /// Scan the path to load a creative content
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        CreativeContent Scan(string path, string mimeType);
    }
}