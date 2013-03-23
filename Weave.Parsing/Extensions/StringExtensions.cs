using System;
using System.Text.RegularExpressions;

namespace Weave.Parsing
{
    internal static class StringExtensions
    {
        public static bool IsImageUrl(this string url)
        {
            if (url == null)
                return false;

            return url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAudioFileUrl(this string url)
        {
            if (url == null)
                return false;

            return url.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVideoFileUrl(this string url)
        {
            if (url == null)
                return false;

            return url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsWellFormed(this string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public static string ParseImageUrlFromHtml(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            Regex r = new Regex(@"src=(?:\""|\')?(?<imgSrc>[^>]*[^/].(?:jpg|png))(?:\""|\')?");

            Match m = r.Match(html);
            if (m.Success && m.Groups.Count > 1 && m.Groups[1].Captures.Count > 0)
            {
                return m.Groups[1].Captures[0].Value;
            }

            return null;
        }
    }
}
