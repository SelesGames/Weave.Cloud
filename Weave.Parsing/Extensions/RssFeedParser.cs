using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using Weave.Parsing.Intermediates;

namespace Weave.Parsing
{
    public static class RssFeedParser
    {
        static readonly XNamespace atom = "http://www.w3.org/2005/Atom";

        public static IEnumerable<IEntryIntermediate> ToRssIntermediates(this MemoryStream ms)
        {
            var elements = ParseUsingCustomParser(ms);

            if (elements != null && elements.Any())
                return elements;

            else
            {
                ms.Position = 0;
                elements = ParseUsingSyndicationFeed(ms);
            }

            return elements;
        }




        #region Utilize the custom Weave parser - only handles true RSS feeds

        static IEnumerable<IEntryIntermediate> ParseUsingCustomParser(Stream stream)
        {
            var doc = XDocument.Load(stream, LoadOptions.None);

            if (doc.Root.Name == atom + "feed")
                return doc.Descendants(atom + "entry").Select(o => new AtomIntermediate(o));
            else
                return doc.Descendants("item").Select(o => new Rss20Intermediate(o));
        }

        #endregion




        #region Utilize the built-in .NET SyndicationFeed class as a fail-safe mechanism

        static IEnumerable<IEntryIntermediate> ParseUsingSyndicationFeed(Stream stream)
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                var feed = SyndicationFeed.Load(reader);
                return feed.Items.Select(item => new SyndicationIntermediate(item)).ToList();
            }
        }

        #endregion
    }
}