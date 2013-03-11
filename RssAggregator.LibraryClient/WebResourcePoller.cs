using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LibraryClient
{
    public class ConditionalHttpClient<T>
    {
        string eTag, lastModified;
        string resourceUrl;
        Func<Stream, T> map;
        IEqualityComparer<T> comparer;


        public T LatestValue { get; private set; }
        public TimeSpan RequestTimeout { get; set; }


        public ConditionalHttpClient(string resourceUrl, Func<Stream, T> map, IEqualityComparer<T> comparer = null)
        {
            this.resourceUrl = resourceUrl;
            this.map = map;
            this.comparer = comparer;

            RequestTimeout = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// Queries the resource url to see if there was an update
        /// </summary>
        /// <returns>A boolean specifying whether the resource was updated</returns>
        public async Task<bool> CheckForUpdate()
        {
            var handler = new SelesGames.WebApi.HttpClientCompressionHandler();
            //{
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            //};

            var request = new HttpClient(handler);

            request.Timeout = RequestTimeout;


            #region CONDITIONAL GET

            if (!string.IsNullOrEmpty(eTag))
            {
                request.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", eTag);
            }

            if (!string.IsNullOrEmpty(lastModified))
            {
                request.DefaultRequestHeaders.TryAddWithoutValidation("If-Modified-Since", lastModified);
            }

            #endregion


            var response = await request.GetAsync(resourceUrl).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return false;
            }

            else if (response.StatusCode == HttpStatusCode.OK)
            {
                T updatedResource;

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    updatedResource = map(stream);
                }

                if (comparer != null && comparer.Equals(updatedResource, LatestValue))
                {
                    return false;
                }
                else
                {
                    LatestValue = updatedResource;
                    SetConditionalHeaders(response);
                    return true;
                }
            }

            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }

        void SetConditionalHeaders(HttpResponseMessage o)
        {
            eTag = o.Headers.GetValueForHeader("ETag");
            lastModified = o.Content.Headers.GetValueForHeader("Last-Modified");
        }
    }

    public static class HttpResponseMessageExtensions
    {
        public static string GetValueForHeader(this HttpHeaders headers, string header)
        {
            IEnumerable<string> headerValues;
            if (headers.TryGetValues(header, out headerValues))
                return headerValues.FirstOrDefault();
            else
                return null;
        }
    }

    public class WebResourcePoller<T> : IDisposable, IObservable<T>
    {
        ConditionalHttpClient<T> client;
        TimeSpan pollingInterval;
        IDisposable disposeHandle;
        Subject<T> resourceUpdated = new Subject<T>();


        public WebResourcePoller(TimeSpan pollingInterval, ConditionalHttpClient<T> client)
        {
            this.client = client;
            this.pollingInterval = pollingInterval;

            InitializeTimer();
        }

        void InitializeTimer()
        {
            disposeHandle = Observable.Interval(pollingInterval).Subscribe(_ => Update(), ex => { ; });
        }

        async Task Update()
        {
            try
            {
                if (await client.CheckForUpdate())
                    resourceUpdated.OnNext(client.LatestValue);
            }
#if DEBUG
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }
#else
            catch { }
#endif
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return resourceUpdated.Subscribe(observer);
        }

        public void Dispose()
        {
            if (disposeHandle != null)
                disposeHandle.Dispose();
        }
    }
}
