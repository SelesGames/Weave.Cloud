using Common.Caching;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class NLevelIconUrlCache : IBasicCache<string, Task<string>>
    {
        NLevelCache<string, Task<string>> cache;

        public NLevelIconUrlCache(params IExtendedCache<string, Task<string>>[] caches)
        {
            this.cache = new NLevelCache<string, Task<string>>(caches);
        }

        public Task<string> Get(string key)
        {
            return cache.GetOrAdd(key, GetNullString);
        }

        Task<string> GetNullString(string url)
        {
            return Task.FromResult<string>(null);
        }
    }
}
