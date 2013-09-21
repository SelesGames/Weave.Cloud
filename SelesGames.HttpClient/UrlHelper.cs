using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public static class UrlHelper
    {
        public static async Task<string> GetFinalRedirectLocation(string url, int cycleLimit = 5)
        {
            int cycleCount = 0;

            string previousRedirect = url;
            var currentRedirect = await GetRedirectOrOriginalUri(url);

            while (currentRedirect != previousRedirect)
            {
                // if a cycle is detected, return the original url
                if (cycleCount++ > cycleLimit)
                    return url;

                previousRedirect = currentRedirect;
                currentRedirect = await GetRedirectOrOriginalUri(currentRedirect);
            }

            return currentRedirect;
        }

        static async Task<string> GetRedirectOrOriginalUri(string url)
        {
            var client = new System.Net.Http.HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            //var request = HttpWebRequest.CreateHttp(url);
            //request.AllowAutoRedirect = false;
            //request.Method = "HEAD";

            //var response = (HttpWebResponse)await request.GetResponseAsync();
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
                //var movedTo = response.Headers[HttpResponseHeader.Location];
                var movedTo = GetLocationOrNull(response);
                if (!string.IsNullOrWhiteSpace(movedTo))
                    return movedTo;
            }

            return url;
        }

        static string GetLocationOrNull(HttpResponseMessage response)
        {
            if (response == null || EnumerableEx.IsNullOrEmpty(response.Headers) || response.Headers.Location == null)
                return null;

            return response.Headers.Location.OriginalString;
        }
    }
}
