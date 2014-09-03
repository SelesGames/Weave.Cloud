using Common.Azure.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Common.Azure.SmartBlobClient
{
    public class SmartBlobClient : AzureBlobClient
    {
        MediaTypeFormatterCollection formatters;

        public string DefaultContentType { get; set; }
        public bool? UseCompressionByDefault { get; set; }




        #region Constructors

        public SmartBlobClient(string storageAccountName, string key, bool useHttps)
            : base(storageAccountName, key, useHttps)
        {
            formatters = CreateDefaultMediaTypeFormatters();
            DefaultContentType = "application/json";
        }

        public SmartBlobClient(CloudStorageAccount account)
            : base(account)
        {
            formatters = CreateDefaultMediaTypeFormatters();
            DefaultContentType = "application/json";
        }

        #endregion




        #region Generic Get

        public async Task<BlobResult<T>> Get<T>(
            string container,
            string blobName,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            try
            {
                using (var content = await this.GetBlobContent(container, blobName, accessCondition, options, operationContext))
                {
                    var result = await ReadContent<T>(content);
                    result.BlobName = blobName;
                    return result;
                }
            }
            catch(StorageException storageException)
            {
                return new BlobResult<T> { BlobName = blobName, StorageException = storageException };
            }
        }

        public async Task<BlobResult<T>> Get<T>(string container, string blobName, RequestProperties properties)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            try
            {
                using (var content = await this.GetBlobContent(container, blobName, properties))
                {
                    var result = await ReadContent<T>(content);
                    result.BlobName = blobName;
                    return result;
                }
            }
            catch (StorageException storageException)
            {
                return new BlobResult<T> { BlobName = blobName, StorageException = storageException };
            }
        }

        #endregion




        #region Generic Save

        public async Task Save<T>(
            string container,
            string blobName,
            T obj,
            BlobProperties blobProperties = null,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            blobProperties = blobProperties ?? new BlobProperties();

            blobProperties.ContentEncoding = blobProperties.ContentEncoding ??
                ((UseCompressionByDefault.HasValue && UseCompressionByDefault.Value) ? "gzip" : null);

            blobProperties.ContentType = blobProperties.ContentType ?? DefaultContentType;
            var formatter = GetBestTypeFormatterForContentType<T>(blobProperties.ContentType);

            using (var ms = new MemoryStream())
            {
                await formatter.WriteToStreamAsync(typeof(T), obj, ms, null, null);
                ms.Position = 0;
                await this.SaveBlobContent(container, blobName, ms, blobProperties, accessCondition, options, operationContext);
            }
        }

        public async Task Save<T>(string container, string blobName, T obj, WriteRequestProperties properties)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            properties.UseCompression = properties.UseCompression ?? UseCompressionByDefault;

            properties.ContentType = properties.ContentType ?? DefaultContentType;
            var formatter = GetBestTypeFormatterForContentType<T>(properties.ContentType);

            using (var ms = new MemoryStream())
            {
                await formatter.WriteToStreamAsync(typeof(T), obj, ms, null, null);
                ms.Position = 0;
                await this.SaveBlobContent(container, blobName, ms, properties);
            }
        }

        #endregion




        #region helper methods

        async Task<BlobResult<T>> ReadContent<T>(BlobContent content)
        {
            try
            {
                var contentType = content.Properties.ContentType;

                MediaTypeHeaderValue mediaHeader;
                if (!MediaTypeHeaderValue.TryParse(contentType, out mediaHeader))
                {
                    throw new Exception(string.Format("Invalid ContentType returned by the call to Get<T> in SmartBlobClient: {0}", contentType));
                }

                var deserializer = FindReadFormatter<T>(mediaHeader);

                var result = await deserializer.ReadFromStreamAsync(typeof(T), content.Content, null, null);
                var value = (T)result;
                return new BlobResult<T> { Content = content, Value = value };
            }
            catch(Exception ex)
            {
                var serializationException = new SerializationException(ex);
                return new BlobResult<T> { SerializationException = serializationException };
            }
        }

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

        MediaTypeFormatter GetBestTypeFormatterForContentType<T>(string contentType)
        {
            MediaTypeFormatter formatter = null;

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                MediaTypeHeaderValue mediaHeader;
                if (!MediaTypeHeaderValue.TryParse(contentType, out mediaHeader))
                {
                    throw new Exception(string.Format("Invalid ContentType returned by the call to Save<T> in SmartBlobClient: {0}", contentType));
                }

                formatter = FindWriteFormatter<T>(mediaHeader);
            }
            else
            {
                formatter = formatters.First();
            }

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
            var jsonFormatter = (JsonMediaTypeFormatter)collection.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return collection;
        }

        #endregion
    }
}
