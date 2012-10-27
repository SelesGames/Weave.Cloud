using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace SelesGames.Rest
{
    public abstract class RestClient<T>
    {
        public bool UseGzip { get; set; }

        public Task<T> GetAsync(string url, CancellationToken cancellationToken)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            return request.GetResponseAsync()
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                        throw new AggregateException(string.Format("Error calling {0}", url), task.Exception.InnerExceptions);

                    var response = (HttpWebResponse)task.Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new WebException("Status code was not OK", null, WebExceptionStatus.UnknownError, response);

                    T result;

                    using (var stream = response.GetResponseStream())
                    {
                        var contentEncoding = response.Headers["Content-Encoding"];
                        if (UseGzip || "gzip".Equals(contentEncoding, StringComparison.OrdinalIgnoreCase))
                        {
                            using (var gzip = new GZipStream(stream, CompressionMode.Decompress, false))
                            {
                                result = ReadObject(gzip);
                                gzip.Close();
                            }
                        }
                        else
                        {
                            result = ReadObject(stream);
                        }
                        stream.Close();
                    }
                    return result;
                },
                cancellationToken);
        }

        protected abstract T ReadObject(Stream stream);
    }
}
