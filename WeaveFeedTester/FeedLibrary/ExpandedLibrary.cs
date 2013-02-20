using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SelesGames.Rest;
using weave;

namespace Weave.FeedLibrary
{
    public class ExpandedLibrary
    {
        string libraryUrl;

        public Lazy<Task<List<FeedSource>>> Feeds { get; private set; }

        public ExpandedLibrary(string libraryUrl)
        {
            this.libraryUrl = libraryUrl;
            Feeds = new Lazy<Task<List<FeedSource>>>(GetFeedsFromWeb);
        }




        #region Load Feeds and Categories XML files

        async Task<List<FeedSource>> GetFeedsFromWeb()
        {
            if (Uri.IsWellFormedUriString(libraryUrl, UriKind.Absolute))
            {
                var client = new LinqToXmlRestClient<List<FeedSource>> { UseGzip = true };
                var feeds = await client.GetAndParseAsync(libraryUrl, Parse, CancellationToken.None);
                return feeds;
            }
            else
            {
                var xdoc = XDocument.Load(libraryUrl);
                return Parse(xdoc.Root);
            }
        }

        List<FeedSource> Parse(XElement doc)
        {
            return doc.Descendants("Feed")
                .Select(feed =>
                    new FeedSource
                    {
                        Category = feed.Parent.Attribute("Type").ValueOrDefault(),
                        FeedName = feed.Attribute("Name").ValueOrDefault(),
                        FeedUri = feed.ValueOrDefault(),
                        ArticleViewingType = ParseArticleViewingType(feed),
                    })
                .ToList();
        }

        ArticleViewingType ParseArticleViewingType(XElement feed)
        {
            var avt = feed.Attribute("ViewType");
            if (avt == null || string.IsNullOrEmpty(avt.Value))
                return ArticleViewingType.Mobilizer;

            var type = avt.Value;

            //if (type.Equals("rss", StringComparison.OrdinalIgnoreCase))
            //    return ArticleViewingType.Local;

            //else if (type.Equals("exrss", StringComparison.OrdinalIgnoreCase))
            //    return ArticleViewingType.LocalOnly;

            if (type.Equals("ieonly", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.InternetExplorerOnly;

            else if (type.Equals("exmob", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.MobilizerOnly;

            else if (type.Equals("ie", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.InternetExplorer;

            else
                return ArticleViewingType.Mobilizer;
        }

        #endregion
    }
}
