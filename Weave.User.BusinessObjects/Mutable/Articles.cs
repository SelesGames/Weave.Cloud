using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public class Articles
    {
        UserIndex user;

        internal Articles(UserIndex user)
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
                .OfType<NewsItemIndex>();

            foreach (var newsItem in newsItems)
                newsItem.HasBeenViewed = true;
        }

        public void MarkCategoryRead(string category)
        {
            var indices = user.FeedIndices
                .OfCategory(category)
                .SelectMany(o => o.NewsItemIndices)
                .OfType<NewsItemIndex>();

            foreach (var newsItem in indices)
                newsItem.HasBeenViewed = true;
        }

        public void MarkFeedRead(Guid feedId)
        {
            var indices = user.FeedIndices
                .Where(o => o.Id == feedId)
                .SelectMany(o => o.NewsItemIndices)
                .OfType<NewsItemIndex>();

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

        NewsItemIndex FindNewsItem(Guid newsItemId)
        {
            return user.FeedIndices
                .SelectMany(o => o.NewsItemIndices)
                .FirstOrDefault(o => o.Id == newsItemId);
        }

        #endregion
    }
}
