using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.Mobilizer.Core.Service;
using Weave.Readability;

namespace Weave.Mobilizer.Core.Cache
{
    public class AzureStorageCache : IExtendedCache<string, Task<ReadabilityResult>>
    {
        AzureClient azureClient;
        SerialDisposable timerDisposeHandle = new SerialDisposable();
        bool isRunningDeleteOperation = false;

        public AzureStorageCache(AzureClient azureClient)
        {
            this.azureClient = azureClient;
        }

        public TimeSpan CacheTTL { get; private set; }
        public TimeSpan CacheCleanupInterval { get; private set; }

        public async Task<ReadabilityResult> GetOrAdd(string key, Func<string, Task<ReadabilityResult>> valueFactory)
        {
            try
            {
                var existing = await azureClient.Get(key);
                return existing;
            }
            catch (Exception ex)
            {
                // do something with exception;
            }
            var result = await valueFactory(key);
            azureClient.Save(key, result);
            return result;
        }




        #region helper functions for clearing the cache

        public void SetCacheTTLAndCleanupIntervalInHours(double cacheTTL, double cleanupInterval)
        {
            CacheTTL = TimeSpan.FromHours(cacheTTL);
            CacheCleanupInterval = TimeSpan.FromHours(cleanupInterval);
            SubscribeToPulse();
        }

        void SubscribeToPulse()
        {
            timerDisposeHandle.Disposable = Observable.Interval(CacheCleanupInterval).Subscribe(notUsed => Pulse());
        }

        async void Pulse()
        {
            if (isRunningDeleteOperation)
                return;

            isRunningDeleteOperation = true;

            try
            {
                await azureClient.DeleteOlderThan(CacheTTL);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            isRunningDeleteOperation = false;
        }

        #endregion
    }
}
