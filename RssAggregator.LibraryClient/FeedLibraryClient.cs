using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Weave.RssAggregator.LibraryClient
{
    public class FeedLibraryClient
    {
        string libraryUrl;
        ConditionalHttpClient<List<FeedSource>> client;

        public List<FeedSource> Feeds { get; private set; }
        public event EventHandler FeedsUpdated;

        public FeedLibraryClient(string libraryUrl)
        {
            this.libraryUrl = libraryUrl;
            client = new ConditionalHttpClient<List<FeedSource>>(libraryUrl, Parse);
            //ListenForChanges();
        }

        public async Task LoadFeedsAsync()
        {
            if (Uri.IsWellFormedUriString(libraryUrl, UriKind.Absolute))
            {
                if (client.LatestValue != null)
                    Feeds = client.LatestValue;

                else
                {
                    if (await client.CheckForUpdate())
                        Feeds = client.LatestValue;
                    else
                        throw new Exception("unable to load feeds!");
                }
            }
            else
            {
                var xdoc = XDocument.Load(libraryUrl);
                Feeds = Parse(xdoc.Root);
            }
        }

        void ListenForChanges()
        {
            var poller = new WebResourcePoller<List<FeedSource>>(TimeSpan.FromSeconds(10), client);
            poller.Subscribe(OnChange);
        }

        void OnChange(List<FeedSource> feeds)
        {
            Feeds = feeds;
            if (FeedsUpdated != null)
                FeedsUpdated(this, EventArgs.Empty);
        }




        #region Load Feeds and Categories XML files

        List<FeedSource> Parse(Stream stream)
        {
            var doc = XElement.Load(stream);
            return Parse(doc);
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
                        Instructions = feed.Attribute("in").ValueOrDefault(),
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
