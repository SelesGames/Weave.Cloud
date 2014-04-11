using System;
using System.Collections.Generic;
using Store = Weave.User.DataStore.v2;

namespace Weave.User.BusinessObjects.v2
{
    public class NewsItemStateCache
    {
        Store.NewsItemStateCache cache;

        public NewsItemStateCache() : this(new Store.NewsItemStateCache()) { }

        public NewsItemStateCache(Store.NewsItemStateCache cache)
        {
            this.cache = cache ?? new Store.NewsItemStateCache();
        }

        public Guid UserId { get; set; }

        public Store.NewsItemStateCache Inner
        {
            get { return cache; }
        }

        public IEnumerable<NewsItemState> MatchingIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                NewsItemState temp;
                if (TryGet(id, out temp))
                    yield return temp;
            }
        }

        public bool TryGet(Guid id, out NewsItemState state)
        {
            var key = CreateKey(id);

            state = new NewsItemState();

            Store.NewsItemState temp;
            if (cache.TryGetValue(key, out temp))
            {
                state.HasBeenViewed = temp.HasBeenViewed;
                state.IsFavorite = temp.IsFavorite;
                return true;
            }
            return false;
        }

        public NewsItemState Get(Guid id)
        {
            var key = CreateKey(id);
            var temp = cache[key];
            return new NewsItemState
            {
                HasBeenViewed = temp.HasBeenViewed,
                IsFavorite = temp.IsFavorite,
            };
        }

        string CreateKey(Guid guid)
        {
            return guid.ToString("N");
        }
    }
}
