using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects.v2.ArticleDeletionTimes;

namespace Weave.User.BusinessObjects.v2
{
    public class NewsCleanupMediator
    {
        UserInfo user;
        MasterNewsItemCollection allNews;
        NewsItemStateCache cache;

        public NewsCleanupMediator(UserInfo user, MasterNewsItemCollection allNews, NewsItemStateCache cache)
        {
            this.user = user;
            this.allNews = allNews;
            this.cache = cache;
        }

        public void DeleteOldNews()
        {
            var markedReadTimes = new ArticleDeleteTimesForMarkedRead();
            var unreadTimes = new ArticleDeleteTimesForUnread();

            var markedReadExpiry = markedReadTimes.GetByDisplayName(user.ArticleDeletionTimeForMarkedRead).Span;
            var unreadExpiry = unreadTimes.GetByDisplayName(user.ArticleDeletionTimeForUnread).Span;

            var now = DateTime.UtcNow;

            var newsGen = new ExtendedNewsItemsMediator(cache);

            foreach (var feed in user.Feeds)
            {
                IEnumerable<NewsItem> feedsNews;
                
                if (allNews.TryGetValue(feed.Id, out feedsNews))
                {
                    if (allNews != null)
                    {
                        allNews[feed.Id] = newsGen
                            .GetExtendedInfo(feedsNews)
                            .Where(o => IsNewsKept(
                                newsItem: o,
                                now: now,
                                markedReadExpiry: markedReadExpiry,
                                unreadExpiry: unreadExpiry))
                            .Select(o => o.NewsItem)
                            .ToList();
                    }
                }                        
            }
        }

        bool IsNewsKept(ExtendedNewsItem newsItem, DateTime now, TimeSpan markedReadExpiry, TimeSpan unreadExpiry)
        {
            var age = now - newsItem.NewsItem.UtcPublishDateTime;

            return
                newsItem.IsFavorite ||
                newsItem.IsNew() ||
                (!newsItem.HasBeenViewed && (age < unreadExpiry)) ||
                (newsItem.HasBeenViewed && (age < markedReadExpiry));
        }
    }
}
