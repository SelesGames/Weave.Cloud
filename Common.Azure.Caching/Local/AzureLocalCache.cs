using Microsoft.ApplicationServer.Caching;
using System.Collections.Concurrent;

namespace Common.Azure.Caching.Local
{
    public class AzureLocalCache
    {
        DataCache outerCache;
        ConcurrentDictionary<string, AzureCacheItem> localCache = new ConcurrentDictionary<string, AzureCacheItem>();

        public AzureLocalCache(DataCache outerCache)
        {
            this.outerCache = outerCache;
        }




        #region Get

        public object Get(string key)
        {
            var localCopy = localCache.GetOrAdd(key, GetFromOuterCache);

            // localCopy.IsNew evaluates to true when object comes from the outer cache
            if (localCopy.IsNew)
            {
                localCopy.IsNew = false;
            }
            // check to see if the DataCache copy is newer
            else
            {
                var version = localCopy.Version;
                var outerCopy = outerCache.GetIfNewer(key, ref version);
                
                // if outerCopy is not null, then the item was in fact updated, and we should replace the copy in the localCache as well
                if (outerCopy != null)
                {
                    localCopy = new AzureCacheItem(outerCopy, version);
                    localCache.AddOrUpdate(key, localCopy, (_, __) => localCopy);
                }
            }

            return localCopy.Value;
        }

        AzureCacheItem GetFromOuterCache(string key)
        {
            DataCacheItemVersion version;

            var outerCopy = outerCache.Get(key, out version);
            if (outerCopy == null)
            {
                throw new AzureCacheMissException(key);
            }

            var localCopy = new AzureCacheItem(outerCopy, version) { IsNew = true };
            return localCopy;
        }

        #endregion




        #region Put

        public void Put<T>(string key, T val)
        {
            localCache.AddOrUpdate(
                key, 
                _ => Add(key, val), 
                (_, localCopy) => Update(key, val, localCopy)
            );
        }

        AzureCacheItem Add(string key, object val)
        {
            DataCacheItemVersion version;
            version = outerCache.Put(key, val);
            return new AzureCacheItem(val, version);
        }

        AzureCacheItem Update(string key, object val, AzureCacheItem localCopy)
        {
            DataCacheItemVersion oldVersion, newVersion;

            oldVersion = localCopy.Version;
            newVersion = outerCache.Put(key, val, oldVersion);

            // if newVersion is null, then another update happened and our versionNumber was old.  So we have to grab the updated version
            if (newVersion == null)
            {
                var outerCopy = outerCache.Get(key, out newVersion);
                val = outerCopy;
            }

            return new AzureCacheItem(val, newVersion);
        }

        #endregion
    }
}





//public T Get<T>(string key) where T : class
//{
//    var localCopy = lookup.GetValueOrDefault<string, AzureCacheItem>(key);
//    if (localCopy != null)
//    {
//        var version = localCopy.Version;
//        var outerCopy = outerCache.GetIfNewer(key, ref version);
//        if (outerCopy != null)
//        {
//            localCopy.Value = outerCopy;
//            localCopy.Version = version;
//        }
//    }
//    else
//    {
//        DataCacheItemVersion version;
//        var outerCopy = outerCache.Get(key, out version);
//        localCopy = new AzureCacheItem { Version = version, Value = outerCopy };
//        lookup.Add(key, localCopy);
//    }

//    return localCopy.GetValue<T>();
//}