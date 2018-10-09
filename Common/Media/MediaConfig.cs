namespace Lucent.Common.Media
{
    /// <summary>
    /// POCO Configuration section
    /// </summary>
    public class MediaConfig
    {
        /// <summary>
        /// Gets the path to ffmpeg
        /// </summary>
        public string FFmpegPath { get; set; } = "/opt/ffmpeg/bin/ffmpeg";
    }
}