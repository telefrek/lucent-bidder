using System;
using System.Drawing;
using System.IO;
using Lucent.Common.Entities;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace Lucent.Common.Media
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaScanner : IMediaScanner
    {
        ILogger<MediaScanner> _log;
        MediaConfig _config;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="options"></param>
        public MediaScanner(ILogger<MediaScanner> log, IOptions<MediaConfig> options)
        {
            _log = log;
            _config = options.Value;
        }

        /// <inheritdoc />
        public CreativeContent Scan(string path, string mimeType)
        {
            try
            {
                if (File.Exists(path))
                {
                    var content = new CreativeContent
                    {
                        ContentLocation = path,
                        MimeType = mimeType,
                    };

                    switch (mimeType.ToLowerInvariant().Split("/")[0])
                    {
                        case "image":
                            if (TryLoadImage(content))
                                return content;
                            return null;
                        case "video":
                            if (TryLoadMedia(content))
                                return content;
                            return null;
                        default:
                            if (TryLoadMedia(content) || TryLoadImage(content))
                                return content;
                            return null;
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to load media {0} ({1})", path, mimeType);
            }

            return null;
        }

        bool TryLoadMedia(CreativeContent content)
        {

            var media = new MediaFile { Filename = content.ContentLocation };
            using (var engine = new Engine(_config.FFmpegPath))
            {
                try
                {
                    engine.GetMetadata(media);
                    content.Duration = (int)media.Metadata.Duration.TotalSeconds;
                    if (media.Metadata.VideoData != null)
                    {
                        content.ContentType = ContentType.Video;
                        content.BitRate = media.Metadata.VideoData.BitRateKbs.GetValueOrDefault();
                        content.Codec = media.Metadata.VideoData.Format;
                        var frame = media.Metadata.VideoData.FrameSize;
                        if (frame.Contains("x"))
                        {
                            var hw = frame.Split("x");
                            content.W = int.Parse(hw[0]);
                            content.H = int.Parse(hw[1]);
                        }
                        content.MimeType = content.MimeType ?? frame;
                    }
                    else
                    {
                        content.ContentType = ContentType.Audio;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    _log.LogWarning(e, "Failed to load {0}", content.ContentLocation);
                }
            }

            return false;
        }

        bool TryLoadImage(CreativeContent content)
        {
            var fInfo = new FileInfo(content.ContentLocation);
            try
            {
                using (var fStream = fInfo.Open(FileMode.Open))
                using (var img = Image.Load(fStream))
                {
                    _log.LogInformation("Loaded {0} : {1}x{2}", content.ContentLocation, img.Height, img.Width);
                    content.H = img.Height;
                    content.W = img.Width;
                    content.ContentType = ContentType.Banner;
                    return true;
                }
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Failed to load {0}", content.ContentLocation);
                _log.LogWarning("File Info: {0} ({1})", fInfo.ToString(), fInfo.Length);
            }

            return false;
        }
    }
}