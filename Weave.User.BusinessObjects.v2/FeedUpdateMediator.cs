using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.User.BusinessObjects.v2.Comparers;
using Weave.User.BusinessObjects.v2.ServiceClients;

namespace Weave.User.BusinessObjects.v2
{
    public class FeedUpdateMediator
    {
        readonly Feed feed;
        readonly MasterNewsItemCollection newsCollection;
        readonly NewsServer client;

        bool isUpdating = false;

        static readonly int TRIM = 100;
        static readonly NewsItemTitleComparer newsItemTitleComparer = new NewsItemTitleComparer(168d);

        // Read-only properties
        public Task CurrentRefresh { get; private set; }

        public FeedUpdateMediator(Feed feed, MasterNewsItemCollection newsCollection, NewsServer client)
        {
            if (feed == null) throw new ArgumentNullException("feed");
            if (newsCollection == null) throw new ArgumentNullException("news");
            if (client == null) throw new ArgumentNullException("client");

            this.feed = feed;
            this.newsCollection = newsCollection;
            this.client = client;
        }




        #region Refresh News

        public void RefreshNews(Action<Exception> onError = null)
        {
            if (isUpdating)
                return;

            CurrentRefresh = refreshNews(onError);
        }

        async Task refreshNews(Action<Exception> onError = null)
        {
            if (isUpdating)
                return;

            isUpdating = true;

            var updatedRequest = new FeedRequest
            {
                Id = feed.Id.ToString(),
                Etag = feed.Etag,
                Url = feed.Uri,
                LastModified = feed.LastModified,
                MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
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

            IEnumerable<NewsItem> news;

            if (!newsCollection.TryGetValue(feed.Id, out news))
                news = new List<NewsItem>();

            var previousNews = news;

            var updatedNews = update.News.Select(Convert).ToList();

            foreach (var newsItem in updatedNews)
            {
                newsItem.OriginalDownloadDateTime = now;
                newsItem.Feed = feed;
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

            newsCollection[feed.Id] = news;

            feed.NewsItemIds = news.Select(o => o.Id).ToList();
            feed.LastRefreshedOn = now;
            feed.IconUri = update.IconUri;
            feed.Etag = update.Etag;
            feed.LastModified = update.LastModified;
            feed.MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
            UpdateTeaserImage(news);
        }

        void UpdateTeaserImage(IEnumerable<NewsItem> news)
        {
            if (EnumerableEx.IsNullOrEmpty(news))
                return;

            feed.TeaserImageUrl = news
                .OrderByDescending(o => o.UtcPublishDateTime)
                .Where(o => o.HasImage)
                .Select(o => o.GetBestImageUrl())
                .FirstOrDefault();
        }

        #endregion




        #region Convert functions

        static Image Convert(RssAggregator.Core.DTOs.Outgoing.Image o)
        {
            return new Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        static NewsItem Convert(RssAggregator.Core.DTOs.Outgoing.NewsItem o)
        {
            return new NewsItem
            {
                Id = o.Id,
                //FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                UtcPublishDateTimeString = o.PublishDateTime,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                HasBeenViewed = false,
                IsFavorite = false,
                Image = o.Image == null ? null : Convert(o.Image),
            };
        }

        #endregion
    }
}
