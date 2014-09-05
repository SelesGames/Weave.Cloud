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

        public MobilizerServiceClient(string token)
        {
            this.token = token;
        }

        public async Task<MobilizerResult> Get(string url, bool stripLeadImage = false)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Not a valid url");

            string append = "ipf";
            var fullUrl = new UriBuilder(SERVICE_URL + append)
                .AddParameter("url", url)
                .AddParameter("stripLeadImage", stripLeadImage)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<MobilizerResult>(fullUrl, CancellationToken.None);
            return result;
        }

        public async Task Post(string url, MobilizerResult article)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Not a valid url");

            string append = "ipf";
            var fullUrl = new UriBuilder(SERVICE_URL + append)
                .AddParameter("token", token)
                .AddParameter("url", url)
                .ToString();

            var client = CreateClient();
            using (var response = await client.PostAsync(fullUrl, article, CancellationToken.None)) { }
        }

        SmartHttpClient CreateClient()
        {
            return new SmartHttpClient(ContentEncoderSettings.Json, CompressionSettings.AcceptEncoding);// | CompressionSettings.ContentEncoding);
        }
    }
}
