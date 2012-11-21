using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace Weave.RssAggregator.Core.Parsing
{
    internal static class RssFeedParser
    {
        static readonly XNamespace atom = "http://www.w3.org/2005/Atom";



        #region Utilize the custom Weave parser - only handles true RSS feeds

        static IEnumerable<IRssIntermediate> ParseUsingCustomParser(this Stream stream)
        {
            List<IRssIntermediate> elements = new List<IRssIntermediate>();
            try
            {
                var doc = XDocument.Load(stream, LoadOptions.None);

                if (doc.Root.Name == atom + "feed")
                    return doc.Descendants(atom + "entry").Select(o => new AtomIntermediate(o));
                else
                    return doc.Descendants("item").Select(o => new Rss20Intermediate(o));
            }
            catch { }
            return elements;
        }

        #endregion

        //static IEnumerable<XElement> GetChildContentElements(XmlReader reader)
        //{
        //    // move to first child
        //    reader.Read();

        //    while (true)
        //    {
        //        // skip whitespace between elements
        //        reader.MoveToContent();

        //        // break on end document section
        //        if (reader.LocalName == "sectPr")
        //            yield break;

        //        yield return (XElement)XElement.ReadFrom(reader);
        //    }
        //}




        #region Utilize the built-in .NET SyndicationFeed class as a fail-safe mechanism

        static IEnumerable<IRssIntermediate> ParseUsingSyndicationFeed(this Stream stream)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    var feed = SyndicationFeed.Load(reader);
                    return feed.Items.Select(item => new SyndicationIntermediate(item)).ToList();
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex.ToString());
                return new List<IRssIntermediate>();
            }
        }

        #endregion




        public static IEnumerable<IRssIntermediate> ToRssIntermediates(this Stream stream)
        {
            try
            {
                IEnumerable<IRssIntermediate> elements;

                using (var ms = stream.ToMemoryStream())
                {
                    elements = ParseUsingCustomParser(ms);

                    if (elements != null && elements.Any())
                        return elements;

                    else
                    {
                        ms.Position = 0;
                        elements = ParseUsingSyndicationFeed(ms);
                    }

                    ms.Close();
                }

                return elements ?? new List<IRssIntermediate>();
            }
            catch (Exception)
            {
                return new List<IRssIntermediate>();
            }
        }

        public static IEnumerable<Tuple<DateTime, IRssIntermediate>> ToRssIntermediatesWithDate(this IEnumerable<IRssIntermediate> elements)
        {
            foreach (var element in elements)
            {
                var dateTime = element.GetTimeStamp();
                if (dateTime.Item1)
                    yield return Tuple.Create(dateTime.Item2, element);
            }
        }
    }
}