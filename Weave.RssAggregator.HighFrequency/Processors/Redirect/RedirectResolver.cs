using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class RedirectResolver : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        public bool IsHandledFully { get { return false; } }

        public Task ProcessAsync(HighFrequencyFeedUpdateDto o)
        {
            return Task.WhenAll(o.Entries.Select(ProcessEntry));
        }

        async Task ProcessEntry(EntryWithPostProcessInfo e)
        {
            try
            {
                var link = e.Link;
                var finalLinkLocation = await GetFinalRedirectLocation(link);
                if (link != finalLinkLocation)
                {
                    e.Link = finalLinkLocation;
                }
            }
            catch { }
        }

        Task<string> GetFinalRedirectLocation(string url, int recursionLimit = 5)
        {
            return GetFinalRedirectLocation(url, 0, recursionLimit);
        }

        async Task<string> GetFinalRedirectLocation(string url, int recurseDepth, int recursionLimit)
        {
            if (recurseDepth >= recursionLimit)
            {
                throw new Exception("Potential HTTP Redirect cycle detected");
            }

            var request = HttpWebRequest.CreateHttp(url);
            request.AllowAutoRedirect = false;
            request.Method = "HEAD";

            var response = (HttpWebResponse)await request.GetResponseAsync();
            var statusCode = response.StatusCode;

            if (
                   statusCode == HttpStatusCode.MultipleChoices     // 300
                || statusCode == HttpStatusCode.Ambiguous           // 300
                || statusCode == HttpStatusCode.MovedPermanently    // 301
                || statusCode == HttpStatusCode.Moved               // 301
                || statusCode == HttpStatusCode.Found               // 302
                || statusCode == HttpStatusCode.Redirect            // 302
                || statusCode == HttpStatusCode.SeeOther            // 303
                || statusCode == HttpStatusCode.RedirectMethod      // 303
                || statusCode == HttpStatusCode.TemporaryRedirect   // 307
                || statusCode == HttpStatusCode.RedirectKeepVerb    // 307
                || (int)statusCode == 308)                          // Permanent Redirect 308, part of experimental RFC proposal
            {
                var movedTo = response.Headers[HttpResponseHeader.Location];
                return await GetFinalRedirectLocation(movedTo, recurseDepth + 1, recursionLimit);
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                return url;
            }
            else
            {
                throw new WebException("Unexpected response", null, WebExceptionStatus.UnknownError, response);
            }
        }
    }
}
