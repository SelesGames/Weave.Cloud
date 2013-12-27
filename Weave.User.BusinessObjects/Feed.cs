using Common.TimeFormatting;
using SelesGames.Common;
using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.User.BusinessObjects.ServiceClients;

namespace Weave.User.BusinessObjects
{
    public class Feed
    {
        bool isUpdating = false;
        List<NewsItem> previousNews;
        List<NewsItem> news;
        object syncObject = new object();

        public UserInfo User { get; set; }
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public string Category { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public DateTime LastRefreshedOn { get; set; }
        public DateTime MostRecentEntrance { get; set; }
        public DateTime PreviousEntrance { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
        public List<NewsItem> News
        {
            get { return news; }
            set
            {
                news = value;
                UpdateTeaserImage();
            }
        }


        public Task CurrentRefresh { get; private set; }
        public string TeaserImageUrl { get; private set; }


        public void EnsureGuidIsSet()
        {
            if (Guid.Empty.Equals(Id))
                Id = CryptoHelper.ComputeHashUsedByMobilizer(Uri);
        }

        public void RefreshNews(NewsServer client)
        {
            if (isUpdating)
                return;

            CurrentRefresh = refreshNews(client);
        }

        async Task refreshNews(NewsServer client)
        {
            if (isUpdating)
                return;

            isUpdating = true;

            if (client == null)
                throw new Exception("no NewsServer was registered via the Service Resolver");

            var updatedRequest = new FeedRequest
            {
                Id = Id.ToString(),
                Etag = Etag,
                Url = Uri,
                LastModified = LastModified,
                MostRecentNewsItemPubDate = MostRecentNewsItemPubDate,
            };

            try
            {
                var update = await client.GetFeedResultAsync(updatedRequest).ConfigureAwait(false);
                HandleUpdate(update);
            }
            catch (Exception exception)
            {
                DebugEx.WriteLine(exception.Message);
            }

            isUpdating = false;
        }




        #region Helper functions for handling an update or update exception

        void HandleUpdate(FeedResult update)
        {
            if (update == null ||
                update.Status != FeedResultStatus.OK || 
                EnumerableEx.IsNullOrEmpty(update.News))
                return;

            previousNews = news;

            DeleteNewsOlderThan(update.OldestNewsItemPubDate);
            AddNews(update.News);

            if (news.IsSetEqualTo(previousNews, new NewsItemEqualityById()))
                return;

            LastRefreshedOn = DateTime.UtcNow;
            IconUri = update.IconUri;
            Etag = update.Etag;
            LastModified = update.LastModified;
            MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
            UpdateTeaserImage();
        }

        void DeleteNewsOlderThan(string date)
        {
            if (EnumerableEx.IsNullOrEmpty(News))
                return;

            var tryGetOldestDate = date.TryGetUtcDate();
            if (tryGetOldestDate.Item1)
            {
                var oldestPubDate = tryGetOldestDate.Item2;
                lock (syncObject)
                {
                    // keep all news that is newer than the oldest pub date, as well as all favorited news
                    var correctedNews = News.Where(o => o.IsFavorite || o.UtcPublishDateTime >= oldestPubDate).ToList();
                    News = correctedNews;
                }
            }
        }

        void AddNews(IEnumerable<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem> newNews)
        {
            if (newNews == null || !newNews.Any())
                return;

            var newNewsInCorrectFormat = newNews
                .Where(o => !DoesAnyExistingNewsItemMatch(o))
                .Select(o => o.Convert<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>(Converters.Converters.Instance))
                .Where(o => !o.FailedToParseUtcPublishDateTime)
                .ToList();

            AddNewNewsItems(newNewsInCorrectFormat);
        }

        bool DoesAnyExistingNewsItemMatch(Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem newNewsItem)
        {
            if (EnumerableEx.IsNullOrEmpty(News))
                return false;

            return News.Any(newsItem => newsItem.Id.Equals(newNewsItem.Id));
        }

        void AddNewNewsItems(IEnumerable<NewsItem> newsToAdd)
        {
            if (newsToAdd == null || !newsToAdd.Any())
                return;

            var originalDownloadDateTime = DateTime.UtcNow;
            foreach (var newsItem in newsToAdd)
            {
                newsItem.OriginalDownloadDateTime = originalDownloadDateTime;
                newsItem.Feed = this;
            }

            lock (syncObject)
            {
                if (News == null)
                    News = new List<NewsItem>();

                News.InsertRange(0, newsToAdd);
            }
        }

        void UpdateTeaserImage()
        {
            if (EnumerableEx.IsNullOrEmpty(News))
                return;

            TeaserImageUrl = News
                .OrderByDescending(o => o.UtcPublishDateTime)
                .Where(o => o.HasImage)
                .Select(o => o.GetBestImageUrl())
                .FirstOrDefault();
        }

        class NewsItemEqualityById : IEqualityComparer<NewsItem>
        {
            public bool Equals(NewsItem x, NewsItem y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(NewsItem obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        #endregion
    }
}
