using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Common.Azure.SmartBlobClient
{
    public class SmartBlobClient : IAzureBlobClient
    {
        MediaTypeFormatterCollection formatters;
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

        public string BlobEndpoint { get { return blobClient.BlobEndpoint; } }


        #endregion




        public SmartBlobClient(string storageAccountName, string key, string container, bool useHttps)
        {
            blobClient = new AzureBlobStreamClient(storageAccountName, key, container, useHttps);
            ContentType = ContentEncoderSettings.Json;
            formatters = CreateDefaultMediaTypeFormatters();
        }

        public async Task<T> Get<T>(string blobId)
        {
            using (var content = await blobClient.Get(blobId))
            {
                var contentType = content.Properties.ContentType;
                var mediaHeader = new MediaTypeHeaderValue(contentType);
                var deserializer = FindReadFormatter<T>(mediaHeader);

                var result = await deserializer.ReadFromStreamAsync(typeof(T), content.Content, null, null);
                return (T)result;
            }
        }

        public async Task Save<T>(string blobId, T obj)
        {
            using (var ms = new MemoryStream())
            {
                var mediaHeader = new MediaTypeHeaderValue(ContentType);
                var serializer = FindWriteFormatter<T>(mediaHeader);

                await serializer.WriteToStreamAsync(typeof(T), obj, ms, null, null);
                ms.Position = 0;
                await blobClient.Save(blobId, ms);
            }
        }

        public Task Delete(string blobId)
        {
            return blobClient.Delete(blobId);
        }




        #region helper methods

        MediaTypeFormatter FindReadFormatter<T>(MediaTypeHeaderValue mediaType)
        {
            MediaTypeFormatter formatter = null;

            var type = typeof(T);
            if (mediaType != null)
            {
                formatter = formatters.FindReader(type, mediaType);
            }

            if (formatter == null)
                throw new Exception(string.Format("unable to find a valid MediaTypeFormatter that matches {0}", mediaType));

            return formatter;
        }

        MediaTypeFormatter FindWriteFormatter<T>(MediaTypeHeaderValue mediaType)
        {
            MediaTypeFormatter formatter = null;

            var type = typeof(T);
            if (mediaType != null)
            {
                formatter = formatters.FindWriter(type, mediaType);
            }

            if (formatter == null)
                throw new Exception(string.Format("unable to find a valid MediaTypeFormatter that matches {0}", mediaType));

            return formatter;
        }

        static MediaTypeFormatterCollection CreateDefaultMediaTypeFormatters()
        {
            var collection = new MediaTypeFormatterCollection();
            collection.Add(new SelesGames.WebApi.Protobuf.ProtobufFormatter());
            return collection;
        }

        #endregion
    }
}
