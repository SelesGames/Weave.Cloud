using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2
{
    public class NewsItemStateCache
    {
        Weave.User.DataStore.v2.NewsItemStateCache cache;

        public NewsItemStateCache(Weave.User.DataStore.v2.NewsItemStateCache cache)
        {
            this.cache = cache;
        }

        public bool TryGet(Guid id, out NewsItemState state)
        {
            var key = CreateKey(id);

            state = new NewsItemState();

            DataStore.v2.NewsItemState temp;
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
