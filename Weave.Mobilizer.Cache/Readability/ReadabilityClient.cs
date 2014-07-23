using SelesGames.HttpClient;
using System.Threading;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache.Readability
{
    public class ReadabilityClient : IMobilizer
    {
        readonly string urlTemplate = "https://www.readability.com/api/content/v1/parser?token={0}&url={1}";
        readonly string token;

        public ReadabilityClient(string token)
        {
            this.token = token;
        }

        public Task<MobilizerResult> Mobilize(string url)
        {
            var fullUrl = string.Format(urlTemplate, token, url);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("calling {0}", url), "READABILITY");
#endif
            return new SmartHttpClient(ContentEncoderSettings.Json, CompressionSettings.ContentEncoding | CompressionSettings.AcceptEncoding)
                .GetAsync<MobilizerResult>(fullUrl, CancellationToken.None);
        }
    }
}
