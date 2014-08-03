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

        public NewsItemIndex MarkRead(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return null;

            newsItem.HasBeenViewed = true;
            return newsItem;
        }

        public NewsItemIndex MarkUnread(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return null;

            newsItem.HasBeenViewed = false;
            return newsItem;
        }

        public IEnumerable<NewsItemIndex> MarkRead(IEnumerable<Guid> newsItemIds)
        {
            if (newsItemIds == null)
                return new List<NewsItemIndex>(0);

            var indices = newsItemIds
                .Select(FindNewsItem)
                .OfType<NewsItemIndex>();

            foreach (var newsItem in indices)
                newsItem.HasBeenViewed = true;

            return indices;
        }

        public IEnumerable<NewsItemIndex> MarkCategoryRead(string category)
        {
            var indices = user.FeedIndices
                .OfCategory(category)
                .SelectMany(o => o.NewsItemIndices)
                .OfType<NewsItemIndex>();

            foreach (var newsItem in indices)
                newsItem.HasBeenViewed = true;

            return indices;
        }

        public IEnumerable<NewsItemIndex> MarkFeedRead(Guid feedId)
        {
            var indices = user.FeedIndices
                .Where(o => o.Id == feedId)
                .SelectMany(o => o.NewsItemIndices)
                .OfType<NewsItemIndex>();

            foreach (var newsItem in indices)
                newsItem.HasBeenViewed = true;

            return indices;
        }

        public NewsItemIndex AddFavorite(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return null;

            newsItem.IsFavorite = true;
            return newsItem;
        }

        public NewsItemIndex RemoveFavorite(Guid newsItemId)
        {
            var newsItem = FindNewsItem(newsItemId);
            if (newsItem == null)
                return null;

            newsItem.IsFavorite = false;
            return newsItem;
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
