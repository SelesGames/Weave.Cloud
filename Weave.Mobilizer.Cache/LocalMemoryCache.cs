using Common.Caching;
using System;
using System.Threading.Tasks;
using Weave.Services.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache
{
    public class LocalMemoryCache : LRUCache<string, Task<MobilizerResult>>, IExtendedCache<string, Task<MobilizerResult>>  // LocalMemoryCache<string, Task<MobilizerResult>> { }
    {
        public LocalMemoryCache()
            : base(10000)
        { }

        public Task<MobilizerResult> GetOrAdd(string key, Func<string, Task<MobilizerResult>> valueFactory)
        {
            var result = base.Get(key);
            if (result != null)
                return result;
            else
            {
                result = valueFactory(key);
                base.AddOrUpdate(key, result);
                return result;
            }
        }
    }
}