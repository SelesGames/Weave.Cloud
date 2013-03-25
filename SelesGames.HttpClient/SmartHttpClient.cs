using Common.Net.Http.Compression;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public class SmartHttpClient : System.Net.Http.HttpClient
    {
        MediaTypeFormatterCollection formatters;
        ContentEncoderSettings encoderSettings;
        CompressionSettings compressionSettings;
        


        public SmartHttpClient(CompressionSettings compressionSettings = CompressionSettings.OnRequest | CompressionSettings.OnContent)
            : this(CreateDefaultMediaTypeFormatters(), ContentEncoderSettings.Json, compressionSettings) { }

        public SmartHttpClient(ContentEncoderSettings encoderSettings, CompressionSettings compressionSettings = CompressionSettings.OnRequest | CompressionSettings.OnContent)
            : this(CreateDefaultMediaTypeFormatters(), encoderSettings, compressionSettings) { }

        public SmartHttpClient(
            MediaTypeFormatterCollection formatters,
            ContentEncoderSettings encoderSettings,
            CompressionSettings compressionSettings = CompressionSettings.OnRequest | CompressionSettings.OnContent)

            : base(new HttpClientCompressionHandler())
        {
            this.formatters = formatters;
            this.encoderSettings = encoderSettings;
            this.compressionSettings = compressionSettings;

            var accept = encoderSettings.Accept;
            if (!string.IsNullOrEmpty(accept))
                DefaultRequestHeaders.TryAddWithoutValidation("Accept", accept);

            if (compressionSettings.HasFlag(CompressionSettings.OnRequest))
                DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", new[] { "gzip", "deflate" });
        }


        public Task<T> GetAsync<T>(string url)
        {
            return GetAsync<T>(url, CancellationToken.None);
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            var response = await GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<T>(formatters).ConfigureAwait(false);
            return result;
        }

        public Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj)
        {
            return PostAsync<TPost, TResult>(url, obj, CancellationToken.None);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
            var mediaType = new MediaTypeHeaderValue(encoderSettings.ContentType);
            var formatter = FindFormatter<TPost>(mediaType);

            var response = await this.PostAsync<TPost>(url, obj, formatter, mediaType, cancelToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<TResult>(formatters).ConfigureAwait(false);
            return result;
        }

        public async Task PostAsync<TPost>(string url, TPost obj, CancellationToken cancelToken)
        {
            var mediaType = new MediaTypeHeaderValue(encoderSettings.ContentType);
            var formatter = FindFormatter<TPost>(mediaType);

            var response = await this.PostAsync<TPost>(url, obj, formatter, mediaType, cancelToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }




        #region helper methods

        MediaTypeFormatter FindFormatter<T>(MediaTypeHeaderValue mediaType)
        {
            MediaTypeFormatter formatter = null;

            var type = typeof(T);
            if (mediaType != null)
            {
                formatter = formatters.FindReader(type, mediaType);
            }

            if (formatter == null)
            {
                formatter = formatters.FirstOrDefault();
            }

            if (formatter == null)
                throw new Exception("unable to find a valid MediaTypeFormatter, and none exist to use as default");

            return formatter;
        }

        static MediaTypeFormatterCollection CreateDefaultMediaTypeFormatters()
        {
            return new MediaTypeFormatterCollection();
        }

        #endregion
    }
}