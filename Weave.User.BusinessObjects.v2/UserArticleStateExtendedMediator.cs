using System;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    /// <summary>
    /// To get the list of news item IDs for a particular Feed or Category of feeds, 
    /// we need to pass in the UserInfo object which will have the list of Feeds
    /// for a user, and each Feed contains a list of FeedIds.
    /// 
    /// Alternatively, we could rework the NewsItemStateCache to store a reference
    /// to the FeedId and the Category of each NewsItem.  However, we would then 
    /// need to modify the Feed update function to modify the state cache in 
    /// instances where the category of a particular Feed was changed
    /// </summary>
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
