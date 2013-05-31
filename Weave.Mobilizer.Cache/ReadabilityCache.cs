using Common.Caching;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;
using Weave.Readability;

namespace Weave.Mobilizer.Cache
{
    public class ReadabilityCache : IBasicCache<string, Task<ReadabilityResult>>
    {
        NLevelCache<string, Task<ReadabilityResult>> cache;
        ReadabilityClient readabilityClient;

        public ReadabilityCache(ReadabilityClient readabilityClient, params IExtendedCache<string, Task<ReadabilityResult>>[] caches)
        {
            this.readabilityClient = readabilityClient;
            this.cache = new NLevelCache<string, Task<ReadabilityResult>>(caches);
        }

        public Task<ReadabilityResult> Get(string key)
        {
            return cache.GetOrAdd(key, readabilityClient.GetAsync);
        }
    }
}