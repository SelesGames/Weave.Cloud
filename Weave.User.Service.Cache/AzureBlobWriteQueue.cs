using Common.Azure;
using System;
using Weave.User.Service.Cache.Extensions;

namespace Weave.User.Service.Cache
{
    public class AzureBlobWriteQueue<T>
    {
        IAzureBlobClient blobClient;

        public AzureBlobWriteQueue(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public async void Add(string key, T obj)
        {
            await TaskEx.Retry(() => blobClient.Save<T>(key, obj), 5, TimeSpan.FromSeconds(4));
        }
    }
}
