using Common.TimeFormatting;
using SelesGames.Common;
using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class Feed
    {
        bool isUpdating = false;
        object syncObject = new object();

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Category { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public DateTime LastRefreshedOn { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
        public List<NewsItem> News { get; set; }


        public Task CurrentRefresh { get; private set; }
        public bool IsModified { get; private set; }


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
            if (update.Status == FeedResultStatus.OK)
            {
                DeleteNewsOlderThan(update.OldestNewsItemPubDate);
                AddNews(update.News);
                //RecalculateNewsHash();

                Etag = update.Etag;
                LastModified = update.LastModified;
                MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
                //SaveToUpdateHistory();
            }

            LastRefreshedOn = DateTime.UtcNow;
            IsModified = true;
        }

        void DeleteNewsOlderThan(string date)
        {
            if (News == null || !News.Any())
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
                .Select(o => o.Convert<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>(Converters.Instance))
                .Where(o => !o.FailedToParseUtcPublishDateTime)
                .ToList();

            AddNewNewsItems(newNewsInCorrectFormat);
        }

        bool DoesAnyExistingNewsItemMatch(Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem newNewsItem)
        {
            if (News == null || News.Count == 0)
                return false;

            return News.Any(newsItem => newsItem.Id.Equals(newNewsItem.Id));
        }

        void AddNewNewsItems(IEnumerable<NewsItem> newsToAdd)
        {
            if (newsToAdd == null || !newsToAdd.Any())
                return;

            var originalDownloadDateTime = DateTime.UtcNow;
            foreach (var newsItem in newsToAdd)
                newsItem.OriginalDownloadDateTime = originalDownloadDateTime;

            lock (syncObject)
            {
                if (News == null)
                    News = new List<NewsItem>();

                News.InsertRange(0, newsToAdd);
            }
        }

        #endregion
    }
}
