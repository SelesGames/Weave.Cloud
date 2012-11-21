using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Weave.RssAggregator.Core.Parsing
{
    internal static class RssParsingXElementExtensions
    {
        //static bool TryParseImageUrl(XElement item, XElement description, out string result)
        //{
        //    var thumb = parseAnyImageInTheDescription(description);

        //    if (string.IsNullOrEmpty(thumb))
        //        thumb = parseMediaContentUrl(item);

        //    if (string.IsNullOrEmpty(thumb))
        //        thumb = parseEnclosure(item);

        //    if (!string.IsNullOrEmpty(thumb))
        //        thumb = Uri.EscapeUriString(thumb);

        //    result = thumb;

        //    return !string.IsNullOrEmpty(thumb) &&
        //        Uri.IsWellFormedUriString(thumb, UriKind.Absolute);
        //}


        static readonly XNamespace content = "http://purl.org/rss/1.0/modules/content/";
        internal static XElement GetContentNode(this XElement item)
        {
            if (item == null)
                return null;

            return item.Element(content + "encoded");
        }


        static readonly XNamespace media = "http://search.yahoo.com/mrss/";
        internal static string GetMediaContentUrl(this XElement item)
        {
            if (item == null)
                return null;

            var mediaTag = item.Element(media + "content");
            if (mediaTag == null)
                return null;

            var thumbUrlTag = mediaTag.Attribute("url");
            if (thumbUrlTag == null)
                return null;

            return thumbUrlTag.Value;
        }


        internal static string GetEnclosureUrl(this XElement item)
        {
            if (item == null)
                return null;

            var mediaTag = item.Element("enclosure");
            if (mediaTag == null)
                return null;

            var thumbUrlTag = mediaTag.Attribute("url");
            if (thumbUrlTag == null)
                return null;

            return thumbUrlTag.Value;
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
