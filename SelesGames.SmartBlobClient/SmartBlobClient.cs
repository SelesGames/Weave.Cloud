using Common.Azure.Blob;
using Microsoft.WindowsAzure.Storage;
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
        public MediaTypeFormatterCollection Formatters { get; private set; }
        public string DefaultContentType { get; set; }
        public bool? UseCompressionByDefault { get; set; }




        #region Constructors

        public SmartBlobClient(string storageAccountName, string key, bool useHttps)
            : base(storageAccountName, key, useHttps)
        {
            Formatters = CreateDefaultMediaTypeFormatters();
            DefaultContentType = "application/json";
        }

        public SmartBlobClient(CloudStorageAccount account)
            : base(account)
        {
            Formatters = CreateDefaultMediaTypeFormatters();
            DefaultContentType = "application/json";
        }

        #endregion




        #region Generic Get

        public async Task<BlobResult<T>> Get<T>(string container, string blobName, RequestProperties properties = null)
        {
            try
            {
                using (var content = await this.GetBlobContent(container, blobName, properties))
                {
                    var response = new BlobResponse(content, Formatters);
                    var result = await response.Read<T>();
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

        public MediaTypeFormatter GetBestTypeFormatterForContentType<T>(string contentType)
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
                formatter = Formatters.First();
            }

            return formatter;
        }

        MediaTypeFormatter FindWriteFormatter<T>(MediaTypeHeaderValue mediaType)
        {
            MediaTypeFormatter formatter = null;

            var type = typeof(T);
            if (mediaType != null)
            {
                formatter = Formatters.FindWriter(type, mediaType);
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
