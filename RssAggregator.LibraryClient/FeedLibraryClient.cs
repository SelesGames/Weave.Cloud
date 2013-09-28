using SelesGames.HttpClient;
using SelesGames.HttpClient.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Weave.RssAggregator.LibraryClient
{
    public class FeedLibraryClient
    {
        string libraryUrl;

        public XElement Xml { get; private set; }
        public List<FeedSource> Feeds { get; private set; }
        public event EventHandler FeedsUpdated;

        public FeedLibraryClient(string libraryUrl)
        {
            this.libraryUrl = libraryUrl;
        }

        public async Task LoadFeedsAsync()
        {
            if (Uri.IsWellFormedUriString(libraryUrl, UriKind.Absolute))
            {
                var client = new SmartHttpClient();
                using (var stream = await client.GetStreamAsync(libraryUrl))
                {
                    OnStreamRead(stream);
                }
            }
            else
            {
                var xdoc = XDocument.Load(libraryUrl, LoadOptions.PreserveWhitespace);
                Xml = xdoc.Root;
                ParseFeedsFromXml();
            }
        }

        public void ListenForChangesToFeeds()
        {
            CreateObservable()
                .Select(o => o.Content.ReadAsStreamAsync().ToObservable())
                .Merge()
                .Subscribe(OnStreamRead);
        }




        #region Load Feeds and Categories XML files

        void ParseFeedsFromXml()
        {
            Feeds = Xml
                .Descendants("Feed")
                .Select(feed =>
                    new FeedSource
                    {
                        Category = feed.Parent.Attribute("Type").ValueOrDefault(),
                        FeedName = feed.Attribute("Name").ValueOrDefault(),
                        IconUrl = feed.Attribute("IconUrl").ValueOrDefault(),
                        FeedUri = feed.ValueOrDefault(),
                        ArticleViewingType = ParseArticleViewingType(feed),
                        Instructions = feed.Attribute("in").ValueOrDefault(),
                    })
                .ToList();

            if (FeedsUpdated != null)
                FeedsUpdated(this, EventArgs.Empty);
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




        #region Auto-Detection of Changed Feeds

        IObservable<HttpResponseMessage> CreateObservable()
        {
            var client = new SmartHttpClient();

            return Observable.Create<HttpResponseMessage>(observer =>
            {
                return client.PollChangesToResource(libraryUrl, TimeSpan.FromMinutes(15), observer);
            });
        }

        void OnStreamRead(Stream stream)
        {
            var xdoc = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            Xml = xdoc.Root;

            ParseFeedsFromXml();
        }

        #endregion
    }
}
