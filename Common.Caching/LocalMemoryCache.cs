using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Common.Caching
{
    public class LocalMemoryCache<TKey, TResult> : IExtendedCache<TKey, TResult>
    {
        ConcurrentDictionary<TKey, CacheEntry<TResult>> cache = new ConcurrentDictionary<TKey, CacheEntry<TResult>>();
        SerialDisposable timerDisposeHandle = new SerialDisposable();

        public TimeSpan CacheTTL { get; private set; }

        public TResult GetOrAdd(TKey key, Func<TKey, TResult> valueFactory)
        {
            //Debug.WriteLine(string.Format("attempting to get {0} from local cache", key), "LOCAL");
            var entry = cache.GetOrAdd(
                key,
                _ =>
                {
                    //Debug.WriteLine(string.Format("{0} added to local cache", key), "LOCAL");
                    var val = valueFactory(key);
                    return new CacheEntry<TResult> { Value = val };
                });
            entry.LastAccess = DateTime.Now;
            return entry.Value;
        }

        public virtual void OnExpire(TResult obj) { }




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
                    CacheEntry<TResult> temp;
                    if (cache.TryRemove(key, out temp))
                        temp = null;

                    OnExpire(entry.Value);
                }
            }
        }

        #endregion
    }
}
