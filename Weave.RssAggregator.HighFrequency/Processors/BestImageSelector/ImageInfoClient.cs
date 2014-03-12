using SelesGames.HttpClient;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency.Processors.BestImageSelector
{
    class ImageInfoClient
    {
        readonly string urlFormat = "http://weave-imagecache.cloudapp.net/api/info?url={0}";

        public async Task<ImageInfo> Get(string url)
        {
            var fullUrl = string.Format(urlFormat, url);

            var client = new SmartHttpClient(CompressionSettings.None);

            var info = await client.GetAsync<ImageInfo>(fullUrl);
            return info;
        }
    }
}
