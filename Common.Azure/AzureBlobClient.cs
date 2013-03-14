using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Common.Azure
{
    public abstract class AzureBlobClient<T> : IAzureBlobClient<T>
    {
        AzureBlobStreamClient blobClient;




        #region Public Properties delegate to the underlying AzureBlobStreamClient

        public TimeSpan ReadTimeout
        {
            get { return blobClient.ReadTimeout; }
            set { blobClient.ReadTimeout = value; }
        }
        public TimeSpan WriteTimeout
        {
            get { return blobClient.WriteTimeout; }
            set { blobClient.WriteTimeout = value; }
        }
        public string ContentType
        {
            get { return blobClient.ContentType; }
            set { blobClient.ContentType = value; }
        }
        public bool UseGzipOnUpload
        {
            get { return blobClient.UseGzipOnUpload; }
            set { blobClient.UseGzipOnUpload = value; }
        }

        #endregion




        public AzureBlobClient(string storageAccountName, string key, string container, bool useHttps)
        {
            blobClient = new AzureBlobStreamClient(storageAccountName, key, container, useHttps);
        }

        public async Task<T> Get(string blobId)
        {
            using (var stream = await blobClient.Get(blobId))
            {
                var result = ReadObject(stream);
                return result;
            }
        }

        public async Task<T> Get(string blobId, T defaultValueOnBlobDoesNotExist)
        {
            T result;
            try
            {
                result = await Get(blobId);
            }
            catch (StorageClientException e)
            {
                Debug.WriteLine(e);
                result = defaultValueOnBlobDoesNotExist;
            }
            return result;
        }

        public async Task Save(string blobId, T obj)
        {
            using (var ms = new MemoryStream())
            {
                await WriteObject(ms, obj);
                ms.Position = 0;
                await blobClient.Save(blobId, ms);
            }
        }

        public Task Delete(string blobId)
        {
            return blobClient.Delete(blobId);
        }




        #region overloaded methods that take in a Guid as the filename
        
        public Task<T> Get(Guid blobId)
        {
            return Get(blobId.ToString());
        }
        public Task<T> Get(Guid blobId, T defaultValueOnBlobDoesNotExist) 
        { 
            return Get(blobId.ToString(), defaultValueOnBlobDoesNotExist); 
        }
        public Task Save(Guid blobId, T obj)
        {
            return Save(blobId.ToString(), obj);
        }
        public Task Delete(Guid blobId)
        {
            return Delete(blobId.ToString());
        }

        #endregion




        // Serialization override functions
        protected abstract T ReadObject(Stream stream);
        protected abstract Task WriteObject(Stream stream, T obj);
    }
}