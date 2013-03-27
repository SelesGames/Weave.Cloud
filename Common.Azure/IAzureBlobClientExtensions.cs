﻿using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Common.Azure
{
    public static class IAzureBlobClientExtensions
    {
        public static Task<T> Get<T>(this IAzureBlobClient<T> client, Guid blobId)
        {
            return client.Get(blobId.ToString());
        }

        public static async Task<T> Get<T>(this IAzureBlobClient<T> client, Guid blobId, T defaultValueOnBlobDoesNotExist)
        {
            T result;
            try
            {
                result = await client.Get(blobId);
            }
            catch (StorageClientException e)
            {
                Debug.WriteLine(e);
                result = defaultValueOnBlobDoesNotExist;
            }
            return result;
        }

        public static Task Save<T>(this IAzureBlobClient<T> client, Guid blobId, T obj)
        {
            return client.Save(blobId.ToString(), obj);
        }
        public static Task Delete<T>(this IAzureBlobClient<T> client, Guid blobId)
        {
            return client.Delete(blobId.ToString());
        }
    }
}
