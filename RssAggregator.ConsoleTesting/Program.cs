﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.HighFrequency;
using System.Linq;
using System.Threading;
using System.IO;
using System.Diagnostics;

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

            var lib = new FeedLibraryClient();
            var feeds = lib.GetFeeds("http://weave.blob.core.windows.net/settings/masterfeeds.xml");
            var requests = feeds.Select((o, index) => new FeedRequest { Id = index.ToString(), Url = o.Url });
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
