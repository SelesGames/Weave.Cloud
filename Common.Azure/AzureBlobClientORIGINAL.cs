using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.Azure
{
    public class AzureBlobClient<T>
    {
        CloudStorageAccount account;
        string container;

        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }
        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeOut { get; set; }

        public AzureBlobClient(string storageAccountName, string key, string container, bool useHttps)
        {
            var blobCred = new StorageCredentialsAccountAndKey(storageAccountName, key);
            account = new CloudStorageAccount(blobCred, useHttps);
            this.container = container;

            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.Converters.Add(new IsoDateTimeConverter());

            ReadTimeout = TimeSpan.FromSeconds(8);
            WriteTimeOut = TimeSpan.FromMinutes(1);
        }

        public async Task<T> Get(Guid blobId)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(blobId.ToString());

            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = ReadTimeout;

            using (var ms = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(ms, options);
                ms.Position = 0;
                var result = ReadObject(ms);
                return result;
            }
        }

        public async Task<T> Get(Guid blobId, T defaultValueOnBlobDoesNotExist)
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

        public async Task Save(Guid blobId, T obj)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(blobId.ToString());

            blob.Properties.ContentType = "application/json; charset=utf-8";
            //blob.Properties.ContentEncoding = "gzip";

            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = WriteTimeOut;

            using (var ms = new MemoryStream())
            using (var streamWriter = new StreamWriter(ms, Encoding))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                var serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(jsonTextWriter, obj);
                jsonTextWriter.Flush();
                ms.Position = 0;
                await blob.UploadFromStreamAsync(ms, options);
            }
        }

        public Task Delete(Guid blobId)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(blobId.ToString());

            return blob.DeleteAsync();
        }




        #region JSON Serialization helpers

        T ReadObject(System.IO.Stream stream)
        {
            using (var streamReader = new StreamReader(stream, Encoding))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = JsonSerializer.Create(SerializerSettings);
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        //int bufferSize = 1024 * 1024;

        //void WriteObject(Stream stream, T obj)
        //{
        //    //using (var streamWriter = new StreamWriter(stream, Encoding, bufferSize, true))
        //    using (var streamWriter = new StreamWriter(stream, Encoding))
        //    using (var jsonTextWriter = new JsonTextWriter(streamWriter))
        //    {
        //        var serializer = JsonSerializer.Create(SerializerSettings);
        //        serializer.Serialize(jsonTextWriter, obj);
        //        jsonTextWriter.Flush();
        //    }
        //}

        #endregion
    }
}