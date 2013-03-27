using System;
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
            using (var content = await blobClient.Get(blobId))
            {
                var result = ReadObject(content.Content);
                return result;
            }
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

        // Serialization override functions
        protected abstract T ReadObject(Stream stream);
        protected abstract Task WriteObject(Stream stream, T obj);
    }
}