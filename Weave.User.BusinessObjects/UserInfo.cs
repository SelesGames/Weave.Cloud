using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.ArticleDeletionTimes;

namespace Weave.User.BusinessObjects
{
    public class UserInfo
    {
        List<Feed> feedsList = new List<Feed>();
        string articleDeletionTimeForMarkedRead, articleDeletionTimeForUnread;

        public Guid Id { get; set; }
        public IReadOnlyCollection<Feed> Feeds { get { return feedsList; } }
        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead
        {
            get { return articleDeletionTimeForMarkedRead; }
            set
            {
                // verify that the passed in string is a legit delete time
                var markedReadTimes = new ArticleDeleteTimesForMarkedRead();
                var deleteTime = markedReadTimes.GetByDisplayName(value);
                articleDeletionTimeForMarkedRead = deleteTime.Display;
            }
        }

        public string ArticleDeletionTimeForUnread
        {
            get { return articleDeletionTimeForUnread; }
            set
            {
                // verify that the passed in string is a legit delete time
                var unreadTimes = new ArticleDeleteTimesForUnread();
                var deleteTime = unreadTimes.GetByDisplayName(value);
                articleDeletionTimeForUnread = deleteTime.Display;
            }
        }

        public Task RefreshAllFeeds()
        {
            return new FeedsSubset(feedsList).Refresh();
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

            feeds = feedsList.OfCategory(category);

            if (EnumerableEx.IsNullOrEmpty(feeds))
                throw new Exception(string.Format("No feeds match category: {0}", category));

            return new FeedsSubset(feeds);
        }

        public FeedsSubset CreateSubsetFromFeedIds(IEnumerable<Guid> feedIds)
        {
            if (EnumerableEx.IsNullOrEmpty(feedIds))
                throw new Exception("No feedIds specified");

            var feeds = from f in feedsList
                        join id in feedIds on f.Id equals id
                        select f;

            if (EnumerableEx.IsNullOrEmpty(feeds))
                throw new Exception(
                    string.Format("No feeds match feedIds: {0}", 
                    feedIds.Aggregate(new StringBuilder(), (sb, id) => sb.Append(id + ", ")).ToString()
                    ));

            return new FeedsSubset(feeds);
        }

        #endregion




        #region Add/Remove/Update a feed

        /// <summary>
        /// Adds a feed to the user's collection of feeds
        /// </summary>
        /// <param name="feed">The feed to be added</param>
        /// <param name="trustSource">Will skip checking to see if feed is already present and that Id is set - use for deserialization only</param>
        /// <returns>True if the feed was added, false if the feed was already present or invalid</returns>
        public bool AddFeed(Feed feed, bool trustSource = false)
        {
            if (feed == null) return false;
            if (string.IsNullOrWhiteSpace(feed.Name) || string.IsNullOrWhiteSpace(feed.Uri))
                return false;

            if (feedsList == null) feedsList = new List<Feed>();

            // if we don't trust the Feed was created correctly, verify it's Id and that no existing Feed matches
            if (!trustSource)
            {
                feed.EnsureGuidIsSet();

                // if any existing feed has a matching Id, don't add it
                if (feedsList.Any(o => o.Id.Equals(feed.Id)))
                    return false;
            }

            feedsList.Add(feed);
            feed.User = this;
            return true;
        }

        public void RemoveFeed(Guid feedId)
        {
            if (EnumerableEx.IsNullOrEmpty(feedsList))
                return;

            var matching = feedsList.FirstOrDefault(o => o.Id.Equals(feedId));
            if (matching != null)
            {
                feedsList.Remove(matching);
            }
        }

        public void UpdateFeed(Feed feed)
        {
            if (EnumerableEx.IsNullOrEmpty(feedsList) || feed == null)
                return;
            if (string.IsNullOrWhiteSpace(feed.Name))
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

        public void MarkNewsItemRead(Guid newsItemId)
        {
            MarkNewsItemRead(FindNewsItem(newsItemId));
        }

        public void MarkNewsItemRead(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            newsItem.HasBeenViewed = true;
        }

        public void MarkNewsItemUnread(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return;

            newsItem.HasBeenViewed = false;
        }

        public void MarkNewsItemsSoftRead(IEnumerable<Guid> newsItemIds)
        {
            if (newsItemIds == null || feedsList == null)
                return;

            var lookup = feedsList.AllNews().ToLookup(o => o.Id);

            var newsItems = newsItemIds.Where(o => lookup.Contains(o)).SelectMany(o => lookup[o]).OfType<NewsItem>();

            foreach (var newsItem in newsItems)
                newsItem.HasBeenViewed = true;
        }

        public void MarkCategorySoftRead(string category)
        {
            if (feedsList == null)
                return;

            foreach (var newsItem in feedsList.OfCategory(category).AllNews())
            {
                newsItem.HasBeenViewed = true;
            }
        }

        public void MarkFeedSoftRead(Guid feedId)
        {
            if (feedsList == null)
                return;

            foreach (var newsItem in feedsList.Where(o => o.Id == feedId).AllNews())
            {
                newsItem.HasBeenViewed = true;
            }
        }

        public void AddFavorite(Guid newsItemId)
        {
            AddFavorite(FindNewsItem(newsItemId));
        }

        public void AddFavorite(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            newsItem.IsFavorite = true;
        }

        public void RemoveFavorite(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return;

            newsItem.IsFavorite = false;
        }

        #endregion




        #region Delete Old News

        public void DeleteOldNews()
        {
            var markedReadTimes = new ArticleDeleteTimesForMarkedRead();
            var unreadTimes = new ArticleDeleteTimesForUnread();

            var markedReadExpiry = markedReadTimes.GetByDisplayName(ArticleDeletionTimeForMarkedRead).Span;
            var unreadExpiry = unreadTimes.GetByDisplayName(ArticleDeletionTimeForUnread).Span;

            var now = DateTime.UtcNow;

            foreach (var feed in Feeds)
            {
                feed.News = feed.News == null ? null :
                    feed.News
                        .Where(o => IsNewsKept(
                            newsItem: o, 
                            now: now, 
                            markedReadExpiry: markedReadExpiry, 
                            unreadExpiry: unreadExpiry))
                        .ToList();
            }
        }

        bool IsNewsKept(NewsItem newsItem, DateTime now, TimeSpan markedReadExpiry, TimeSpan unreadExpiry)
        {
            var age = now - newsItem.UtcPublishDateTime;

            return
                newsItem.IsFavorite ||
                newsItem.IsNew() ||
                (!newsItem.HasBeenViewed && (age < unreadExpiry)) ||
                (newsItem.HasBeenViewed && (age < markedReadExpiry));
        }

        #endregion




        #region helper methods

        public NewsItem FindNewsItem(Guid newsItemId)
        {
            if (EnumerableEx.IsNullOrEmpty(feedsList))
                return null;

            var newsItem = feedsList
                .AllNews()
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
