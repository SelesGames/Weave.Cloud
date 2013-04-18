using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class UserInfo
    {
        List<Feed> feedsList = new List<Feed>();

        public Guid Id { get; set; }
        public IReadOnlyList<Feed> Feeds { get { return feedsList; } }
        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public Task RefreshAllFeeds()
        {
            return new FeedsSubset(feedsList).Refresh();
        }

        public bool IsNew(NewsItem newsItem)
        {
            return !newsItem.HasBeenViewed && newsItem.OriginalDownloadDateTime > PreviousLoginTime;
        }

        public IEnumerable<NewsItem> GetLatestArticles()
        {
            return GetTopNewsItems();
        }




        #region Create a feed subset from either a category name or a list of feedIds
        
        public FeedsSubset CreateSubsetFromCategory(string category)
        {
            IEnumerable<Feed> feeds = null;

            if (string.IsNullOrEmpty(category))
                throw new Exception("No category specified");

            else if ("all news".Equals(category, StringComparison.OrdinalIgnoreCase))
                feeds = feedsList;

            else
            {
                feeds = feedsList.OfCategory(category);
            }

            if (feeds == null)
                throw new Exception(string.Format("No feeds match category '{0}", category));

            return new FeedsSubset(feeds);
        }

        public FeedsSubset CreateSubsetFromFeedIds(IEnumerable<Guid> feedIds)
        {
            if (feedIds == null || !feedIds.Any())
                throw new Exception("No feedIds specified");

            var feeds = Feeds.Join(feedIds, o => o.Id, x => x, (o, x) => o).ToList();
            return new FeedsSubset(feeds);
        }

        #endregion




        #region Add/Remove/Update a feed

        public void AddFeed(Feed feed, bool trustSource = false)
        {
            if (feed == null) return;
            if (feedsList == null) feedsList = new List<Feed>();

            // if we don't trust the Feed was created correctly, verify it's Id and that no existing Feed matches
            if (!trustSource)
            {
                feed.EnsureGuidIsSet();

                // if any existing feed has a matching Id, don't add it
                if (feedsList.Any(o => o.Id.Equals(feed.Id)))
                    return;
            }

            feedsList.Add(feed);
        }

        public void RemoveFeed(Guid feedId)
        {
            if (feedsList == null || !feedsList.Any())
                return;

            var matching = feedsList.FirstOrDefault(o => o.Id.Equals(feedId));
            if (matching != null)
            {
                feedsList.Remove(matching);
            }
        }

        public void UpdateFeed(Feed feed)
        {
            if (feedsList == null || !feedsList.Any() || feed == null)
                return;

            var matching = feedsList.FirstOrDefault(o => o.Id.Equals(feed.Id));
            if (matching != null)
            {
                // the only 3 fields the user can change are category, feed name, and article viewing type
                matching.Category = feed.Category;
                matching.Name = feed.Name;
                matching.ArticleViewingType = feed.ArticleViewingType;
            }
        }

        #endregion




        #region Mark NewsItem read/unread

        public async Task MarkNewsItemRead(Guid feedId, Guid newsItemId)
        {
            var newsItem = FindNewsItem(feedId, newsItemId);
            if (newsItem == null)
                return;

            var saved = newsItem.Convert<NewsItem, Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem>(Converters.Instance);
            await ArticleServiceClient.Current.MarkRead(Id, saved);
            newsItem.HasBeenViewed = true;
        }

        public async Task MarkNewsItemUnread(Guid feedId, Guid newsItemId)
        {
            var newsItem = FindNewsItem(feedId, newsItemId);
            if (newsItem == null)
                return;

            await ArticleServiceClient.Current.RemoveRead(Id, newsItemId);
            newsItem.HasBeenViewed = false;
        }

        #endregion




        #region helper methods

        NewsItem FindNewsItem(Guid feedId, Guid newsItemId)
        {
            if (feedsList == null || !feedsList.Any())
                return null;

            var newsItem = feedsList
                .Where(o => o.Id.Equals(feedId))
                .SelectMany(o => o.News)
                .FirstOrDefault(o => o.Id.Equals(newsItemId));

            return newsItem;
        }

        #endregion




        #region helper methods for creating the latest/featured news

        const int pageSize = 8;

        IEnumerable<NewsItem> GetTopNewsItems()
        {
            IEnumerable<NewsItem> pool = Feeds
                .AllOrderedNews()
                .Where(news => !news.HasBeenViewed)
                //.Distinct(NewsItemComparer.Instance)
                .Take(20)
                .ToList();

            pool = CreatePool(pool);
            var topNewsItems = pool.Take(pageSize).ToList();
            return topNewsItems;
        }

        static IEnumerable<NewsItem> CreatePool(IEnumerable<NewsItem> allNewsItems)
        {
            return allNewsItems
                .Select(i => new
                {
                    NewsItem = i,
                    AdjustedSortRating = GetAdjustedForImagePresenceSortRating(i),
                })
                .Select(i =>
                new
                {
                    NewsItem = i.NewsItem,
                    FinalAdjustedSortRating = GetAdjustedForRepetitiveFeedSortRating(i.AdjustedSortRating)
                })
                .OrderByDescending(i => i.FinalAdjustedSortRating)
                .Select(i => i.NewsItem);
        }

        static double GetAdjustedForImagePresenceSortRating(NewsItem i)
        {
            return i.HasImage ? 100d * i.SortRating : i.SortRating;
        }

        static double GetAdjustedForRepetitiveFeedSortRating(double i)
        {
            return i;
        }

        #endregion
    }
}
