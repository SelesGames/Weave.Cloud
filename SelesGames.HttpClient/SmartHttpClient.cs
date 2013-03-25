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
    /// <summary>
    /// A version of HttpClient that reads the Content-Type header to deserialize a web request response into an object.  
    /// It also auto-decompresses responses.
    /// 
    /// By default, requests compressed Json objects
    /// </summary>
    public class SmartHttpClient : System.Net.Http.HttpClient
    {
        MediaTypeFormatterCollection formatters;
        ContentEncoderSettings encoderSettings;
        CompressionSettings compressionSettings;




        #region Constructors

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

        #endregion




        #region Get

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

        #endregion




        #region Post

        public Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj)
        {
            return PostAsync<TPost, TResult>(url, obj, CancellationToken.None);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
            var response = await GetValidPostResponseAsync<TPost>(url, obj, cancelToken);
            var result = await response.Content.ReadAsAsync<TResult>(formatters).ConfigureAwait(false);
            return result;
        }

        public Task PostAsync<TPost>(string url, TPost obj, CancellationToken cancelToken)
        {
            return GetValidPostResponseAsync<TPost>(url, obj, cancelToken);
        }

        async Task<HttpResponseMessage> GetValidPostResponseAsync<T>(string url, T obj, CancellationToken cancelToken)
        {
            var mediaType = new MediaTypeHeaderValue(encoderSettings.ContentType);
            var formatter = FindFormatter<T>(mediaType);

            var content = new ObjectContent<T>(obj, formatter, mediaType);
            content.Headers.ContentType = mediaType;

            var response = await base.PostAsync(url, content, cancelToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        #endregion




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