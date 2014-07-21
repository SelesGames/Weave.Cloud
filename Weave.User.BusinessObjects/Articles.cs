using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects
{
    public class Articles
    {
        UserInfo user;

        internal Articles(UserInfo user)
        {
            this.user = user;
        }




        #region Article State Management

        public void MarkRead(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return;

            newsItem.HasBeenViewed = true;
        }

        public void MarkUnread(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return;

            newsItem.HasBeenViewed = false;
        }

        public void MarkRead(IEnumerable<Guid> newsItemIds)
        {
            if (newsItemIds == null)
                return;

            var newsItems = newsItemIds
                .Select(FindNewsItem)
                .OfType<NewsItem>();

            foreach (var newsItem in newsItems)
                newsItem.HasBeenViewed = true;
        }

        public void MarkCategoryRead(string category)
        {
            var indices = user.Feeds
                .OfCategory(category)
                .AllNews();

            foreach (var newsItem in indices)
                newsItem.HasBeenViewed = true;
        }

        public void MarkFeedRead(Guid feedId)
        {
            var indices = user.Feeds
                .WithId(feedId)
                .AllNews();

            foreach (var newsItem in indices)
                newsItem.HasBeenViewed = true;
        }

        public void AddFavorite(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
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

        NewsItem FindNewsItem(Guid newsItemId)
        {
            return user.Feeds
                .SelectMany(o => o.News)
                .FirstOrDefault(o => o.Id == newsItemId);
        }

        #endregion
    }
}
