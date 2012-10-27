using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.Readability;

namespace Weave.Mobilizer.Core.Cache
{
    public class LocalMemoryCache : IExtendedCache<string, Task<ReadabilityResult>>
    {
        ConcurrentDictionary<string, TimeStampedCacheEntry<Task<ReadabilityResult>>> cache = new ConcurrentDictionary<string, TimeStampedCacheEntry<Task<ReadabilityResult>>>();
        SerialDisposable timerDisposeHandle = new SerialDisposable();

        public TimeSpan CacheTTL { get; private set; }

        public Task<ReadabilityResult> GetOrAdd(string key, Func<string, Task<ReadabilityResult>> valueFactory)
        {
            //Debug.WriteLine(string.Format("attempting to get {0} from local cache", key), "LOCAL");
            var entry = cache.GetOrAdd(
                key,
                _ =>
                {
                    //Debug.WriteLine(string.Format("{0} added to local cache", key), "LOCAL");
                    var task = valueFactory(key);
                    return new TimeStampedCacheEntry<Task<ReadabilityResult>> { Value = task };
                });
            entry.LastAccess = DateTime.Now;
            return entry.Value;
        }




        #region helper functions for clearing the cache

        public void SetCacheTTLInMinutes(int minutes)
        {
            CacheTTL = TimeSpan.FromMinutes(minutes);
            SubscribeToPulse();
        }

        void SubscribeToPulse()
        {
            timerDisposeHandle.Disposable = Observable.Interval(CacheTTL).Subscribe(notUsed => Pulse());
        }

        void Pulse()
        {
            var now = DateTime.Now;
            var cacheEntries = cache.ToList();

            foreach (var kvp in cacheEntries)
            {
                var entry = kvp.Value;
                var key = kvp.Key;
                var elapsed = now - entry.LastAccess;
                if (elapsed > CacheTTL)
                {
                    TimeStampedCacheEntry<Task<ReadabilityResult>> temp;
                    if (cache.TryRemove(key, out temp))
                        temp = null;
                }
            }
        }

        #endregion
    }
}
