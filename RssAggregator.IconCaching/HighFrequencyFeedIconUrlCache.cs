using Common.Caching;
using FeedIconGrabber;
using SelesGames.HttpClient;
using SelesGames.HttpClient.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class HighFrequencyFeedIconUrlCache : 
        IBasicCache<string, Task<string>>, 
        IExtendedCache<string, Task<string>>,
        IDisposable
    {
        readonly string MAPPING_DEF_URL = "http://weave.blob.core.windows.net/settings/iconMap.json";

        bool isCacheLoaded = false;
        Dictionary<string, string> feedIconLookup;
        IDisposable listenerHandle;
        SmartHttpClient client;

        public HighFrequencyFeedIconUrlCache()
        {
            client = new SmartHttpClient();
        }

        public void BeginListeningToResourceChanges()
        {
            if (listenerHandle != null)
                listenerHandle.Dispose();

            listenerHandle = client.PollChangesToResource(
                MAPPING_DEF_URL,
                TimeSpan.FromMinutes(15),
                OnResourceUpdated);
        }

        async void OnResourceUpdated(HttpResponseMessage response)
        {
            var iconMappings = await client.ReadResponseContentAsync<FeedUrlIconMappings>(response);
            feedIconLookup = iconMappings.ToDictionary(o => o.Url, o => o.IconUrl);
            isCacheLoaded = true;
        }




        #region IBasicCache<string, Task<string>>

        public async Task<string> Get(string key)
        {
            int tryMax = 10;
            int tryCount = 0;

            while (!isCacheLoaded)
            {
                if (tryCount >= tryMax)
                {
                    throw new Exception("problem loading high-frequency icons from the resource url");
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
                tryCount++;
            }

            return feedIconLookup[key];
        }

        #endregion




        #region IExtendedCache<string, Task<string>>

        public async Task<string> GetOrAdd(string key, Func<string, Task<string>> valueFactory)
        {
            if (isCacheLoaded)
            {
                if (feedIconLookup.ContainsKey(key))
                {
                    return feedIconLookup[key];
                }
                else
                {
                    return await valueFactory(key);
                }
            }
            else
            {
                try
                {
                    return await Get(key);
                }
                catch { }

                return await valueFactory(key);
            }
        }

        #endregion




        #region IDisposable

        public void Dispose()
        {
            if (listenerHandle != null)
                listenerHandle.Dispose();
        }

        #endregion
    }
}
