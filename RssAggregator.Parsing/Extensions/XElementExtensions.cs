using System.Xml.Linq;

namespace Weave.RssAggregator.Parsing
{
    internal static class XElementExtensions
    {
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
    }
}
