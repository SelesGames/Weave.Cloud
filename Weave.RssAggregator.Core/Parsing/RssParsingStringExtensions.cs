using System;

namespace Weave.RssAggregator.Core.Parsing
{
    internal static class RssParsingStringExtensions
    {
        internal static bool IsImageUrl(this string url)
        {
            if (url == null)
                return false;

            return url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsAudioFileUrl(this string url)
        {
            if (url == null)
                return false;

            return url.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsVideoFileUrl(this string url)
        {
            if (url == null)
                return false;

            return url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsWellFormed(this string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}
