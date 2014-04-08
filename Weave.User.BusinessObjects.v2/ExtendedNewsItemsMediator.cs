using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2
{
    public class ExtendedNewsItemsMediator
    {
        readonly NewsItemStateCache cache;
 
        public ExtendedNewsItemsMediator(NewsItemStateCache cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");

            this.cache = cache;
        }

        public IEnumerable<ExtendedNewsItem> GetExtendedInfo(IEnumerable<NewsItem> news)
        {
            if (news == null) throw new ArgumentNullException("news");

            foreach (var newsItem in news)
            {
                NewsItemState state;
                if (cache.TryGet(newsItem.Id, out state))
                {
                    var extended = new ExtendedNewsItem(newsItem);
                    extended.IsFavorite = state.IsFavorite;
                    extended.HasBeenViewed = state.HasBeenViewed;
                    yield return extended;
                }
            }
        }
    }
}
