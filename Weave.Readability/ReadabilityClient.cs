﻿using SelesGames.Rest;
using System.Threading.Tasks;

namespace Weave.Readability
{
    public class ReadabilityClient
    {
        readonly string urlTemplate = "https://www.readability.com/api/content/v1/parser?token={0}&url={1}";
        readonly string token;

        public ReadabilityClient(string token)
        {
            this.token = token;
        }

        public Task<ReadabilityResult> GetAsync(string url)
        {
            var fullUrl = string.Format(urlTemplate, token, url);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("calling {0}", url), "READABILITY");
#endif
            return new JsonRestClient<ReadabilityResult> { UseGzip = true }
                .GetAsync(fullUrl, System.Threading.CancellationToken.None);
        }
    }
}
