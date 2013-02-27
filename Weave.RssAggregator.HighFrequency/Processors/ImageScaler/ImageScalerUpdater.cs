using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.HighFrequency
{
    public class ImageScalerUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        readonly string urlFormat = "http://weave-imagecache.cloudapp.net/api/cache?url={0}";
        HttpClient client;

        public ImageScalerUpdater()
        {
            IsHandledFully = false;
            client = new HttpClient() 
            { 
                Timeout = TimeSpan.FromMinutes(2),
            };
        }

        public bool IsHandledFully { get; private set; }

        public Task ProcessAsync(HighFrequencyFeedUpdateDto o)
        {
            return Task.WhenAll(o.Entries.Select(ProcessEntry)); 
        }

        async Task ProcessEntry(Entry e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.ImageUrl))
                    return;

                var url = string.Format(urlFormat, HttpUtility.UrlEncode(e.ImageUrl));
                var response = await client.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(response))
                    return;
                
                var urlList = JsonConvert.DeserializeObject<List<string>>(response);
                if (!urlList.Any())
                    return;

                var sd = urlList.FirstOrDefault(o => o.EndsWith("-sd.jpg"));
                if (sd != null)
                    e.ImageUrl = urlList.First();
            }
            catch { }
        }
    }
}
