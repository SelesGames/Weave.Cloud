using SelesGames.HttpClient;
using System;
using System.Threading;
using System.Threading.Tasks;
using Weave.Mobilizer.Contracts;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Client
{
    public class MobilizerServiceClient : IMobilizerService
    {
        string token;
        const string SERVICE_URL = "http://mobilizer.cloudapp.net/";

        //http://mobilizer.cloudapp.net/ipf?url=

        public MobilizerServiceClient(string token)
        {
            this.token = token;
        }

        public async Task<ReadabilityResult> Get(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Not a valid url");

            string append = "ipf";
            var fullUrl = new UriBuilder(SERVICE_URL + append)
                .AddParameter("url", url)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<ReadabilityResult>(fullUrl, CancellationToken.None);
            return result;
        }

        public Task Post(string url, ReadabilityResult article)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Not a valid url");

            string append = "ipf";
            var fullUrl = new UriBuilder(SERVICE_URL + append)
                .AddParameter("token", token)
                .AddParameter("url", url)
                .ToString();

            var client = CreateClient();
            return client.PostAsync(fullUrl, article, CancellationToken.None);
        }

        SmartHttpClient CreateClient()
        {
            return new SmartHttpClient(ContentEncoderSettings.Json, CompressionSettings.OnRequest | CompressionSettings.OnContent);
            //return new SmartHttpClient(ContentEncoderSettings.Protobuf, CompressionSettings.None);
        }
    }
}
