using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.User.BusinessObjects.Comparers;
using Weave.User.BusinessObjects.ServiceClients;

namespace Weave.User.BusinessObjects
{
    public class Feed
    {
        #region Private member variables

        static readonly int TRIM = 100;
        static readonly NewsItemTitleComparer newsItemTitleComparer = new NewsItemTitleComparer(168d);

        bool isUpdating = false;
        List<NewsItem> news;

        #endregion




        #region Public Properties

        public Guid Id { get; set; }
        public UserInfo User { get; set; }
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
        public IReadOnlyList<NewsItem> News
        {
            get { return news; }
            set
            {
                news = value == null ? null : value.ToList();
                UpdateTeaserImage();
            }
        }

        // Read-only properties
        public Task CurrentRefresh { get; private set; }
        public string TeaserImageUrl { get; private set; }

        #endregion




        public void EnsureGuidIsSet()
        {
            if (Guid.Empty.Equals(Id))
                Id = CryptoHelper.ComputeHashUsedByMobilizer(Uri);
        }




        #region Refresh News

        public void RefreshNews(NewsServer client, Action<Exception> onError = null)
        {
            if (isUpdating)
                return;

            CurrentRefresh = refreshNews(client, onError);
        }

        async Task refreshNews(NewsServer client, Action<Exception> onError = null)
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
                if (onError != null)
                    onError(exception);
            }

            isUpdating = false;
        }

        #endregion




        #region Helper functions for handling an update

        void HandleUpdate(FeedResult update)
        {
            if (update == null ||
                update.Status != FeedResultStatus.OK || 
                EnumerableEx.IsNullOrEmpty(update.News))
                return;

            var now = DateTime.UtcNow;
            news = news ?? new List<NewsItem>();
            var previousNews = news;

            var updatedNews = update.News.Convert().ToList();

            foreach (var newsItem in updatedNews)
            {
                newsItem.OriginalDownloadDateTime = now;
                newsItem.Feed = this;
            }

            var mergedNews = news
                .Union(updatedNews, newsItemTitleComparer)
                .OrderByDescending(o => o.IsNew())
                .ThenByDescending(o => o.UtcPublishDateTime)
                .Take(TRIM)
                .ToList();

            news = mergedNews;

            if (news.IsSetEqualTo(previousNews, new NewsItemIdComparer()))
                return;

            LastRefreshedOn = now;
            IconUri = update.IconUri;
            Etag = update.Etag;
            LastModified = update.LastModified;
            MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
            UpdateTeaserImage();
        }

        void UpdateTeaserImage()
        {
            if (EnumerableEx.IsNullOrEmpty(news))
                return;

            TeaserImageUrl = news
                .OrderByDescending(o => o.UtcPublishDateTime)
                .Where(o => o.HasImage)
                .Select(o => o.GetBestImageUrl())
                .FirstOrDefault();
        }

        #endregion
    }
}




#region deprecated, old way of merging in the news


//DeleteNewsOlderThan(update.OldestNewsItemPubDate);
//AddNews(update.News);
//AdjustForDuplicateTitles();
//Trim();


//void DeleteNewsOlderThan(string date)
//{
//    if (EnumerableEx.IsNullOrEmpty(news))
//        return;

//    var tryGetOldestDate = date.TryGetUtcDate();
//    if (tryGetOldestDate.Item1)
//    {
//        var oldestPubDate = tryGetOldestDate.Item2;
//        lock (syncObject)
//        {
//            // keep all news that is newer than the oldest pub date, as well as all favorited news
//            var correctedNews = news.Where(o => o.IsFavorite || o.UtcPublishDateTime >= oldestPubDate).ToList();
//            news = correctedNews;
//        }
//    }
//}

//void AddNews(IEnumerable<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem> newNews)
//{
//    if (newNews == null || !newNews.Any())
//        return;

//    var newNewsInCorrectFormat = newNews
//        .Where(o => !DoesAnyExistingNewsItemMatch(o))
//        .Select(o => o.Convert<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>(Converters.Converters.Instance))
//        .Where(o => !o.FailedToParseUtcPublishDateTime)
//        .ToList();

//    AddNewNewsItems(newNewsInCorrectFormat);
//}

//void AddNewNewsItems(IEnumerable<NewsItem> newsToAdd)
//{
//    if (newsToAdd == null || !newsToAdd.Any())
//        return;

//    var originalDownloadDateTime = DateTime.UtcNow;
//    foreach (var newsItem in newsToAdd)
//    {
//        newsItem.OriginalDownloadDateTime = originalDownloadDateTime;
//        newsItem.Feed = this;
//    }

//    lock (syncObject)
//    {
//        if (news == null)
//            news = new List<NewsItem>();

//        news.InsertRange(0, newsToAdd);
//    }
//}

//bool DoesAnyExistingNewsItemMatch(Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem newNewsItem)
//{
//    if (EnumerableEx.IsNullOrEmpty(news))
//        return false;

//    return news.Any(newsItem => newsItem.Id.Equals(newNewsItem.Id));
//}

//void AdjustForDuplicateTitles()
//{
//    news = news == null ? null : news.Distinct(newsItemTitleComparer).ToList();
//}

//void Trim()
//{
//    news = news == null ? null : news.Take(TRIM).ToList();
//}

#endregion