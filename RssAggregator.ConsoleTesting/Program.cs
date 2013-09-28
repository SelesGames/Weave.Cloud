using Common.Azure.ServiceBus;
using FeedIconGrabber;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Parsing;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.HighFrequency;
using Weave.RssAggregator.LibraryClient;
using Weave.RssAggregator.WorkerRole.Startup;


namespace RssAggregator.ConsoleTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TestIconUrlMappingsFileCreator().Wait();
                //TestFeedsFileWriter().Wait();
                //TestIconDownloader().Wait();
                //TestInvidualFeedIconGrab().Wait();
                //TestFeedIconUrlGrabber().Wait();
                //TestMobilizerUpload().Wait();
                //TestRedirect().Wait();
                //TestBasicFreedRequester().Wait();
                //TestChangeProccer().Wait();
                //TestSub().Wait();
                //FixUnsetBlobs().Wait();
                //TestReceiveServiceBusMessageQueue();
                //TestSendToServiceBusMessageQueue();
                //FuckThisShit().Wait();
                //TestService().Wait();
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }

            Console.WriteLine("all tests passed!");

            while (true)
                Console.ReadLine();
        }

        static async Task TestIconUrlMappingsFileCreator()
        {
            var creator = new FeedUrlIconMappingsCreator();
            await creator.BeginDownload();
        }

        static async Task TestFeedsFileWriter()
        {
            var setter = new XmlIconUrlSetter();
            await setter.RewriteXml();
            Console.Write("done");
        }

        static async Task TestIconDownloader()
        {
            var downloader = new IconDownloader();
            await downloader.BeginDownload();

            Debug.Write(downloader);
        }

        static async Task TestInvidualFeedIconGrab()
        {
            var feed = new Feed { FeedUri = "http://www.pcgamer.com/feed/rss2" };
            await feed.Update();

            var domainUrl = feed.DomainUrl;
            var grabber = new DomainIconAcquirer(domainUrl);
            var url = await grabber.GetIconUrl();

            Debug.Write(url);
        }

        static async Task TestFeedIconUrlGrabber()
        {
            //var feedLibraryClient = new FeedLibraryClient(@"C:\WORK\CODE\SELES GAMES\Weave.Cloud\masterfeeds.xml");
            //await feedLibraryClient.LoadFeedsAsync();

            //var feeds = feedLibraryClient.Feeds
            //    .Where(o => o.Category.Equals("technology", StringComparison.OrdinalIgnoreCase))
            //    .Select(o => new Feed { FeedUri = o.FeedUri, IsAggressiveDomainDiscoveryEnabled = true })
            //    .Take(100)
            //    .ToList();

            var feeds = new List<Feed> { new Feed { FeedUri = "http://www.theverge.com/rss/index.xml", IsAggressiveDomainDiscoveryEnabled = true } };

            var feedsWithIconUrls = new List<FeedWithIconUrl>();
            foreach (var feed in feeds)
            {
                var feedWithIconUrl = await GetIconUrl(feed);
                feedsWithIconUrls.Add(feedWithIconUrl);
            }
            //var feedsWithIconUrls = await Task.WhenAll(feeds.Select(GetIconUrl));

            Debug.Write(feedsWithIconUrls);
        }

        static async Task<FeedWithIconUrl> GetIconUrl(Feed feed)
        {
            string iconUrl = null;

            try
            {
                await feed.Update();
                var grabber = new DomainIconAcquirer(feed.DomainUrl);
                iconUrl = await grabber.GetIconUrl();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return new FeedWithIconUrl { Feed = feed, IconUrl = iconUrl };
        }

        class FeedWithIconUrl
        {
            public Feed Feed { get; set; }
            public string IconUrl { get; set; }

            public override string ToString()
            {
                return string.Format("{0} --> ICON: {1}", Feed.FeedUri, IconUrl);
            }
        }

        static async Task TestMobilizerUpload()
        {
            var client = new MobilizerServiceClient("hxyuiplkx78!ksdfl");
            await client.Post("http://www.espn.com", new Weave.Mobilizer.DTOs.ReadabilityResult
                {
                    author = "test",
                    content = "hello world",
                    title = "asdf",
                    word_count = "not available",
                });

                        //                title = e.Title,
                        //url = e.Link,
                        //date_published = e.UtcPublishDateTimeString,
                        //domain = feed.FeedUri,
                        //content = e.Description,
                        //author = null,
                        //word_count = "not available",
        }

        static async Task TestRedirect()
        {
            var url = "http://feeds.gawker.com/~r/gizmodo/vip/~3/TWQrJP2unx8/how-to-make-a-see-through-margarita-510717690";
            var finalUrl = await GetFinalRedirectLocation(url);
            DebugEx.WriteLine(finalUrl);
        }

        static async Task<string> GetFinalRedirectLocation(string url)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.AllowAutoRedirect = false;
            request.Method = "HEAD";

            var response = (HttpWebResponse)await request.GetResponseAsync();
            if (response.StatusCode == HttpStatusCode.MovedPermanently)
            {
                var movedTo = response.Headers[HttpResponseHeader.Location];
                return await GetFinalRedirectLocation(movedTo);
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                var stream = response.GetResponseStream();
                var memStream = stream.ToMemoryStream();
                DebugEx.WriteLine(memStream.Length);
                return url;
            }
            else
            {
                throw new WebException("Unexpected response", null, WebExceptionStatus.UnknownError, response);
            }
        }

        static async Task TestBasicFreedRequester()
        {
            var feedRequester = new Feed
            {
                FeedUri = "http://feeds.boingboing.net/boingboing/iBag",
            };

            await feedRequester.Update();
            DebugEx.WriteLine(feedRequester);
        }

        static async Task TestChangeProccer()
        {
            var lib = new FeedLibraryClient("http://weave.blob.core.windows.net/settings/testFeeds.xml");
            await lib.LoadFeedsAsync();
            DumpFeeds(lib.Feeds);

            lib.FeedsUpdated += (s, e) =>
            {
                DebugEx.WriteLine("feeds changed");
                DumpFeeds(lib.Feeds);
            };
        }

        static void DumpFeeds(List<FeedSource> feeds)
        {
            foreach (var feed in feeds)
            {
                Console.WriteLine(feed.FeedUri);
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        static async Task TestSub()
        {
            var kernel = new NinjectKernel();
            var connector = kernel.Get<ClientFactory>();

            var client = await connector.CreateSubscriptionClient("FeedUpdatedTopic", "test");

            var message = await client.ReceiveAsync();
            DebugEx.WriteLine(message);
            //await client.AsObservable().Do(message =>
            //{
            //    DebugEx.WriteLine("Body: " + message.GetBody<string>());
            //    DebugEx.WriteLine("MessageID: " + message.MessageId);
            //    //DebugEx.WriteLine("MessageNumber: " + message.Properties["MessageNumber"]);
            //    DebugEx.WriteLine("FeedId: " + message.Properties["FeedId"]);

            //    // Remove message from subscription
            //    //message.Complete();
            //}).ToTask();
        }

        //static async Task FixUnsetBlobs()
        //{
        //    var kernel = new NinjectKernel();
        //    var provider = kernel.Get<IProvider<ITransactionalDatabaseClient>>();

        //    int takeCount = 5;
        //    int skipCount = 0;

        //    while (true)
        //    {
        //        using (var client = provider.Get())
        //        {
        //            //var news = await client.GetAllByCriteria<Sql.NewsItem>(o => o.NewsItemBlob == null || o.UtcPublishDateTimeString == null);
        //            var news = await client.Get<Sql.NewsItem>().ConfigureAwait(false);
        //            news = news.Where(o => o.NewsItemBlob == null || o.UtcPublishDateTimeString == null).Skip(skipCount).Take(takeCount);

        //            if (!news.Any())
        //                return;

        //            foreach (var o in news)
        //            {
        //                var entry = new Entry
        //                {
        //                    Id = o.Id,
        //                    FeedId = o.FeedId,
        //                    Title = o.Title,
        //                    Link = o.Link,
        //                    OriginalPublishDateTimeString = o.OriginalPublishDateTimeString,
        //                    UtcPublishDateTime = o.PublishDateTime,
        //                    VideoUri = o.VideoUri,
        //                    YoutubeId = o.YoutubeId,
        //                    PodcastUri = o.PodcastUri,
        //                    ZuneAppId = o.ZuneAppId,
        //                };

        //                entry.AddImage(o.ImageUrl);

        //                if (o.NewsItemBlob == null)
        //                {
        //                    var newsItem = Convert(entry);

        //                    using (var ms = new MemoryStream())
        //                    {
        //                        Serializer.Serialize(ms, newsItem);
        //                        ms.Position = 0;
        //                        var byteArray = ms.ToArray();
        //                        o.NewsItemBlob = byteArray;
        //                    }
        //                }

        //                if (o.UtcPublishDateTimeString == null)
        //                {
        //                    o.UtcPublishDateTimeString = entry.UtcPublishDateTimeString;
        //                }

        //                await client.SubmitChanges();
        //            }
        //        }

        //        //skipCount += takeCount;
        //    }
        //}

        static NewsItem Convert(Entry e)
        {
            return new NewsItem
            {
                Title = e.Title,
                Link = e.Link,
                ImageUrl = e.GetImageUrl(),
                PublishDateTime = e.UtcPublishDateTimeString,
                Description = null,//entry.Description,
                VideoUri = e.VideoUri,
                YoutubeId = e.YoutubeId,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                Id = e.Id,
                FeedId = e.FeedId,
            };
        }

        static void TestReceiveServiceBusMessageQueue()
        {
            QueueConnector.Initialize();
            var client = QueueConnector.OrdersQueueClient;
            BrokeredMessage receivedMessage = null;
            receivedMessage = client.Receive();
            Debug.WriteLine(receivedMessage);
            var body = receivedMessage.GetBody<string>();
            Debug.WriteLine(body);
            receivedMessage.Complete();
        }

        static void TestSendToServiceBusMessageQueue()
        {
            QueueConnector.Initialize();
            var client = QueueConnector.OrdersQueueClient;
            BrokeredMessage message;

            for (int i = 0; i < 10; i++)
            {
                message = new BrokeredMessage("hello world " + i);
                client.Send(message);
            }
        }

        static async Task TestGetLatestForFeedId()
        {
            var kernel = new NinjectKernel();
            var client = kernel.Get<SqlClient>();

            var id = await client.GetLatestForFeedId(Guid.NewGuid());
            Debug.Write(id);
        }

        static async Task TestService()
        {
            var requests = new List<FeedRequest>
            {
                new FeedRequest
                {
                    Id = "1",
                    Url = "http://feeds.arstechnica.com/arstechnica/index"
                },
                new FeedRequest
                {
                    Id = "2",
                    Url = "http://feeds.feedburner.com/CrunchGear"                
                },
            };

            var stringRep = JsonConvert.SerializeObject(requests, Formatting.Indented);


            var protoClient = new HttpClient { Timeout = TimeSpan.FromHours(5) };
            protoClient.DefaultRequestHeaders.Accept.TryParseAdd("application/protobuf");
            protoClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip");

            string url =
//"http://a2d93f4da03c4d9587afb87173a866d9.cloudapp.net/api/weave?xxx={0}";
//"http://127.0.0.1:8086/weave?fsd=true";
"http://127.0.0.1/api/weave";

            var response = await protoClient.PostAsync(url, new StringContent(stringRep, Encoding.UTF8, "application/json"));

            List<FeedResult> results;

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                results = ProtoBuf.Serializer.Deserialize<List<FeedResult>>(gzipStream);
            }

            Debug.WriteLine(results);
        }

        static async Task TimeService()
        {

            var lib = new FeedLibraryClient("http://weave.blob.core.windows.net/settings/masterfeeds.xml");
            await lib.LoadFeedsAsync();
            var feeds = lib.Feeds;

            var requests = feeds.Select((o, index) => new FeedRequest { Id = index.ToString(), Url = o.FeedUri });
            var stringRep = JsonConvert.SerializeObject(requests, Formatting.Indented);

            var jsonClient = new HttpClient { Timeout = TimeSpan.FromHours(5) };
            jsonClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");

            var protoClient = new HttpClient { Timeout = TimeSpan.FromHours(5) };
            protoClient.DefaultRequestHeaders.Accept.TryParseAdd("application/protobuf");

            string url =
"http://a2d93f4da03c4d9587afb87173a866d9.cloudapp.net/api/weave?xxx={0}";
//"http://127.0.0.1:8086/weave?fsd=true";
//"http://127.0.0.1/api/weave";

            var guids = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid());
            var urls = guids.Select(o => string.Format(url, o)).ToList();



            var sw = System.Diagnostics.Stopwatch.StartNew();
            var jsonResponses = await Task.WhenAll(
                urls
                .AsParallel()
                .WithDegreeOfParallelism(10)
                .Select(o => TimeDownloadSpeed(jsonClient, o, stringRep))
            );
            sw.Stop();

            var averageResponseTime = jsonResponses.Sum(o => o.Elapsed.TotalSeconds) / jsonResponses.Length;
            var totalResponseTime = sw.Elapsed.TotalSeconds;
            Debug.WriteLine(string.Format("JSON Average seconds: {0}, Total seconds: {1}", averageResponseTime, totalResponseTime));


            sw = System.Diagnostics.Stopwatch.StartNew();
            var protoResponses = await Task.WhenAll(
                urls
                .AsParallel()
                .WithDegreeOfParallelism(10)
                .Select(o => TimeDownloadSpeed(protoClient, o, stringRep))
            );
            sw.Stop();

            averageResponseTime = protoResponses.Sum(o => o.Elapsed.TotalSeconds) / protoResponses.Length;
            totalResponseTime = sw.Elapsed.TotalSeconds;
            Debug.WriteLine(string.Format("PROTO Average seconds: {0}, Total seconds: {1}", averageResponseTime, totalResponseTime));
        }




        #region Time POST speed

        static async Task<TimingResult<HttpResponseMessage>> TimeDownloadSpeed(HttpClient client, string url, string stringRep)
        {
            HttpResponseMessage response = null;
            Exception exception = null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                //response = await client.GetAsync(url).ConfigureAwait(false);
                response = await client.PostAsync(url, new StringContent(stringRep, Encoding.UTF8, "application/json"));
            }
            catch (Exception e)
            {
                exception = e;
            }
            sw.Stop();
            var threadId = Thread.CurrentThread.ManagedThreadId;
            return new TimingResult<HttpResponseMessage> { Elapsed = sw.Elapsed, Value = response, ThreadId = threadId, Exception = exception };
        }

        #endregion




        #region Timing for Parsing functions

        static async Task<TimingResult<List<FeedResult>>> TimeProtobufParsingSpeed(HttpResponseMessage response)
        {
            List<FeedResult> results = null;
            Exception exception = null;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
                {
                    results = ProtoBuf.Serializer.Deserialize<List<FeedResult>>(gzipStream);
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            sw.Stop();
            var threadId = Thread.CurrentThread.ManagedThreadId;
            return new TimingResult<List<FeedResult>> { Elapsed = sw.Elapsed, Value = results, ThreadId = threadId, Exception = exception };
        }

        static async Task<TimingResult<List<FeedResult>>> TimeJsonParsingSpeed(HttpResponseMessage response)
        {
            List<FeedResult> results = null;
            Exception exception = null;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
                using (var sr = new StreamReader(gzipStream, Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    results = serializer.Deserialize<List<FeedResult>>(jsonReader);
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            sw.Stop();
            var threadId = Thread.CurrentThread.ManagedThreadId;
            return new TimingResult<List<FeedResult>> { Elapsed = sw.Elapsed, Value = results, ThreadId = threadId, Exception = exception };
        }

        #endregion




        class TimingResult<T>
        {
            public TimeSpan Elapsed { get; set; }
            public T Value { get; set; }
            public int ThreadId { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
