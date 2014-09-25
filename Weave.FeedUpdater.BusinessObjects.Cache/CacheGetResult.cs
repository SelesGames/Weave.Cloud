using SelesGames.Common;
using System.Collections.Generic;

namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public class CacheGetMultiResult<T>
    {
        public IEnumerable<CacheGetResult<T>> Results { get; set; }
        public dynamic Meta { get; set; }
    }

    public class CacheGetResult<T>
    {
        public bool HasValue { get { return Value is T; } }
        public T Value { get; set; }
        public string Source { get; set; }
    }
}