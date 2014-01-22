using Common.Net.Http.Compression;
using System;
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

        internal MediaTypeFormatterCollection Formatters { get { return formatters; } }




        #region Constructors

        public SmartHttpClient(CompressionSettings compressionSettings = CompressionSettings.OnRequest | CompressionSettings.OnContent)
            : this(CreateDefaultMediaTypeFormatters(), ContentEncoderSettings.Default, compressionSettings) { }

        public SmartHttpClient(ContentEncoderSettings encoderSettings, CompressionSettings compressionSettings = CompressionSettings.OnRequest | CompressionSettings.OnContent)
            : this(CreateDefaultMediaTypeFormatters(), encoderSettings, compressionSettings) { }

        SmartHttpClient(
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

            return await ReadResponseContentAsync<T>(response).ConfigureAwait(false);
        }

        #endregion




        #region Post

        public Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj)
        {
            return PostAsync<TPost, TResult>(url, obj, CancellationToken.None);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
            var response = await PostAsync(url, obj, cancelToken);
            response.EnsureSuccessStatusCode();

            return await ReadResponseContentAsync<TResult>(response).ConfigureAwait(false);
        }

        public Task<HttpResponseMessage> PostAsync<T>(string url, T obj, CancellationToken cancelToken)
        {
            var mediaType = new MediaTypeHeaderValue(encoderSettings.ContentType);
            var formatter = FindWriteFormatter<T>(mediaType);

            var content = new ObjectContent<T>(obj, formatter, mediaType);

            return base.PostAsync(url, content, cancelToken);
        }

        #endregion




        #region Read Response.Content async public helper method

        public async Task<T> ReadResponseContentAsync<T>(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsAsync<T>(formatters).ConfigureAwait(false);
            return result;
        }

        #endregion




        #region helper methods

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