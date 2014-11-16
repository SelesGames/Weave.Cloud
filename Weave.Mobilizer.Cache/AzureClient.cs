using Common.Azure.Blob;
using Common.Azure.SmartBlobClient;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Weave.Services.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache
{
    public class AzureClient
    {
        readonly bool USE_HTTPS = false;
        readonly string CONTAINER = "articles";

        string account, key;

        public AzureClient(string account, string key)
        {
            this.account = account;
            this.key = key;
        }

        public async Task Save(string url, MobilizerResult result)
        {
            var client = new SmartBlobClient(account, key, USE_HTTPS);

            await client.Save(CONTAINER, url, result, 
                new WriteRequestProperties
                {
                    ContentType = "application/json; charset=utf-8",
                    UseCompression = true,
                    RequestTimeOut = TimeSpan.FromMinutes(3),
                });

            Debug.WriteLine(string.Format("{0} uploaded to azure", url), "AZURE");
        }

        public async Task<MobilizerResult> Get(string url)
        {
            var client = new SmartBlobClient(account, key, USE_HTTPS);

            var result = await client.Get<MobilizerResult>(CONTAINER, url,
                new RequestProperties
                {
                    RequestTimeOut = TimeSpan.FromMinutes(8)
                });

            return result.Value;
        }
    }
}
