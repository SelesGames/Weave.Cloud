using System;
using Weave.RssAggregator.Core.Services.HighFrequency;
using Weave.RssAggregator.Core;

namespace Weave.RssAggregator.WcfTestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //var feedLibraryUrl = "http://weavestorage.blob.core.windows.net/settings/masterfeeds.xml";
            //var feedLibraryUrl = @"D:\WORK\Code\Windows Phone 7\masterfeeds.xml";
            string feedLibraryUrl = null;

            HighFrequencyFeedCacheService.CreateCache(feedLibraryUrl);
            AppSettings.LowFrequencyHttpWebRequestTimeout = TimeSpan.FromMinutes(10);

            Weave.RssAggregator.Core.WcfEndpointCreator.CreateEndpoint("http://localhost:8086");
            Console.WriteLine("Rss Aggregator Service Running!");

            while (true)
                Console.Read();
        }
    }
}
