using Common.Azure.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common.Azure.SmartBlobClient
{
    public class BlobResponse : IDisposable
    {
        bool isDisposed = false;

        public MediaTypeFormatterCollection Formatters { get; private set; }
        public BlobContent BlobContent { get; private set; }

        public BlobResponse(BlobContent blobContent, MediaTypeFormatterCollection formatters)
        {
            if (blobContent == null) throw new ArgumentNullException("blobContent");
            if (formatters == null) throw new ArgumentNullException("formatters");

            this.BlobContent = blobContent;
            this.Formatters = formatters;
        }

        public Stream ReadStream()
        {
            return BlobContent.Content;
        }




        #region Read Stream as object

        public async Task<BlobResult<T>> Read<T>()
        {
            try
            {
                var contentType = BlobContent.Properties.ContentType;

                MediaTypeHeaderValue mediaHeader;
                if (!MediaTypeHeaderValue.TryParse(contentType, out mediaHeader))
                {
                    throw new Exception(string.Format("Invalid ContentType returned by the call to Get<T> in SmartBlobClient: {0}", contentType));
                }

                var deserializer = FindReadFormatter<T>(mediaHeader);

                var result = await deserializer.ReadFromStreamAsync(typeof(T), BlobContent.Content, null, null);
                var value = (T)result;
                return new BlobResult<T> { Content = BlobContent, Value = value };
            }
            catch (Exception ex)
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
                formatter = Formatters.FindReader(type, mediaType);
            }

            if (formatter == null)
                throw new Exception(string.Format("unable to find a valid MediaTypeFormatter that matches {0}", mediaType));

            return formatter;
        }

        #endregion




        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            BlobContent.Dispose();
        }
    }
}
