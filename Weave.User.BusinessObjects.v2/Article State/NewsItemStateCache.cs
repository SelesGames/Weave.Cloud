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

            Store.NewsItemState temp;
            if (cache.TryGetValue(key, out temp))
            {
                state = new NewsItemState(temp);
                state.Key = key;
                //state.HasBeenViewed = temp.HasBeenViewed;
                //state.IsFavorite = temp.IsFavorite;
                return true;
            }

            state = null;
            return false;
        }

        public NewsItemState Get(Guid id)
        {
            var key = CreateKey(id);
            var temp = cache[key];
            return new NewsItemState(temp)
            {
                Key = key,
                //HasBeenViewed = temp.HasBeenViewed,
                //IsFavorite = temp.IsFavorite,
            };
        }

        string CreateKey(Guid guid)
        {
            return guid.ToString("N");
        }

        public bool ContainsKey(Guid id)
        {
            var key = CreateKey(id);
            return cache.ContainsKey(key);
        }

        public void Add(Guid id, NewsItemState state)
        {
            Add(CreateKey(id), state);
        }

        public void Add(string key, NewsItemState state)
        {
            cache.Add(key, new Store.NewsItemState
            {
                HasBeenViewed = state.HasBeenViewed,
                IsFavorite = state.IsFavorite,
            });
        }
    }
}
