using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.v2
{
    public class UserMediator
    {
        NewsItemStateCache newsItemStateCache;
        UserInfo user;

        public UserMediator(UserInfo user, NewsItemStateCache newsItemStateCache)
        {
            this.user = user;
            this.newsItemStateCache = newsItemStateCache;
        }




        #region Mark NewsItem read/unread

        public void MarkNewsItemRead(Guid newsItemId)
        {
            NewsItemState state;
            if (newsItemStateCache.TryGet(newsItemId, out state))
            {
                state.HasBeenViewed = true;
            }
        }

        public void MarkNewsItemRead(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            MarkNewsItemRead(newsItem.Id);
        }

        public void MarkNewsItemUnread(Guid newsItemId)
        {
            NewsItemState state;
            if (newsItemStateCache.TryGet(newsItemId, out state))
            {
                state.HasBeenViewed = false;
            }
        }

        public void MarkNewsItemsSoftRead(IEnumerable<Guid> newsItemIds)
        {
            if (newsItemIds == null)
                return;

            foreach (var state in newsItemStateCache.MatchingIds(newsItemIds))
            {
                state.HasBeenViewed = true;
            }
        }

        public void MarkCategorySoftRead(string category)
        {
            var newsItemIds = user.Feeds
                .FindByCategory(category)
                .SelectMany(o => o.NewsItemIds);

            var states = newsItemStateCache.MatchingIds(newsItemIds);

            foreach (var state in states)
            {
                state.HasBeenViewed = true;
            }
        }

        public void MarkFeedSoftRead(Guid feedId)
        {
            var newsItemIds = user.Feeds
                .FindById(feedId)
                .SelectMany(o => o.NewsItemIds);

            var states = newsItemStateCache.MatchingIds(newsItemIds);

            foreach (var state in states)
            {
                state.HasBeenViewed = true;
            }
        }

        public void AddFavorite(Guid newsItemId)
        {
            NewsItemState state;
            if (newsItemStateCache.TryGet(newsItemId, out state))
            {
                state.IsFavorite = true;
            }
        }

        public void AddFavorite(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            AddFavorite(newsItem.Id);
        }

        public void RemoveFavorite(Guid newsItemId)
        {
            NewsItemState state;
            if (newsItemStateCache.TryGet(newsItemId, out state))
            {
                state.IsFavorite = false;
            }
        }

        #endregion
    }
}
