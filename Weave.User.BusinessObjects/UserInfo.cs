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
        string articleDeletionTimeForMarkedRead, articleDeletionTimeForUnread;

        public Guid Id { get; set; }
        public Feeds Feeds { get; private set; }
        public Articles Articles { get; private set; }
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

        public UserInfo()
        {
            Feeds = new Feeds(this);
            Articles = new Articles(this);
        }

        public Task RefreshAllFeeds()
        {
            return new FeedsSubset(Feeds).Refresh();
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

            feeds = Feeds.OfCategory(category);

            if (EnumerableEx.IsNullOrEmpty(feeds))
                throw new Exception(string.Format("No feeds match category: {0}", category));

            return new FeedsSubset(feeds);
        }

        public FeedsSubset CreateSubsetFromFeedIds(IEnumerable<Guid> feedIds)
        {
            if (EnumerableEx.IsNullOrEmpty(feedIds))
                throw new Exception("No feedIds specified");

            var feeds = from f in Feeds
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
