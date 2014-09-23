
namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public class CacheGetResult<T>
    {
        public bool HasValue { get { return Value is T; } }
        public T Value { get; set; }
        public object Meta { get; set; }
    }
}
