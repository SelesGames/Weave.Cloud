using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace RssAggregator.ConsoleTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TestService().Wait();
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }

            while (true)
                Console.ReadLine();
        }

        static async Task TestService()
        {
            var client = new HttpClient();
            var obj = new List<FeedRequest>
            {
                new FeedRequest
                {
                    Etag = "feqbzy5VhcEZ4UIdFhCS2A3Teqc",
                    Id = "1",
                    LastModified = "Wed, 29 Jun 2011 05:00:03 GMT",
                    MostRecentNewsItemPubDate = "Tue, 28 Jun 2011 22:42:25 -0500",
                    Url = "http://feeds.arstechnica.com/arstechnica/index"
                },
                new FeedRequest
                {
                    Etag = "Si+SRAkb6M2K/t1ZNyau/x1adTk",
                    Id = "11",
                    LastModified = "Tue, 28 Jun 2011 22:35:05 GMT",
                    MostRecentNewsItemPubDate = "Tue, 28 Jun 2011 22:31:17 +0000",
                    Url = "http://feeds.feedburner.com/CrunchGear"                
                },
            };

            var stringRep = JsonConvert.SerializeObject(obj, Formatting.Indented);
            var response = await client.PostAsync(
                "http://127.0.0.1:8086/legacyweave?fsd=true",
                //"http://127.0.0.1/api/weave",
                new StringContent(stringRep, Encoding.UTF8, "application/json"));

            List<FeedResult> result;

            //using (var stream = await response.Content.ReadAsStreamAsync())
            //using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
            //using (var sr = new StreamReader(gzipStream, Encoding.UTF8))
            //using (var jsonReader = new JsonTextReader(sr))
            //{
            //    var serializer = new JsonSerializer();
            //    result = serializer.Deserialize<List<FeedResult>>(jsonReader);
            //}

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                result = ProtoBuf.Serializer.Deserialize<List<FeedResult>>(gzipStream);
            }

            DebugEx.WriteLine(result);
        }
    }
}
