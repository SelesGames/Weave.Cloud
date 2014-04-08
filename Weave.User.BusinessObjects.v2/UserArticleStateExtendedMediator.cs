using System;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public class UserArticleStateExtendedMediator
    {
        NewsItemStateCache newsItemStateCache;
        UserInfo user;

        public UserArticleStateExtendedMediator(UserInfo user, NewsItemStateCache newsItemStateCache)
        {
            this.user = user;
            this.newsItemStateCache = newsItemStateCache;
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
    }
}
