using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.Core.Services;
using Weave.RssAggregator.Core.Services.HighFrequency;

namespace Weave.RssAggregator.Core
{
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = false,
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple
    )]
    public class RssAggregator : IRssAggregator, IPing
    {
        public IAsyncResult BeginGetWeave(Stream feedRequests, string schema, string forceSuppressDescription, AsyncCallback callback, object state)
        {
            try
            {
                var highFrequencyFeedCache = HighFrequencyFeedCacheService.CurrentCache;

                var jsonSerializer = new DataContractJsonSerializer(typeof(List<FeedRequest>));
                var requests = (List<FeedRequest>)jsonSerializer.ReadObject(feedRequests);

                var highFrequencyFeeds = requests.Where(o => highFrequencyFeedCache.Contains(o.Url)).ToList();
                var lowFrequencyFeeds = requests.Except(highFrequencyFeeds).ToList();

                var lowFrequencyResults = lowFrequencyFeeds.Select(o => o.GetNewsAsync()).Merge();
                var highFrequencyResults = highFrequencyFeeds.Select(o => highFrequencyFeedCache.ToFeedResult(o)).ToObservable();

                var fullResults = lowFrequencyResults.Merge(highFrequencyResults)
                    .Aggregate(new List<FeedResult>(), (list, next) =>
                    {
                        list.Add(next);
                        return list;
                    });

                bool isDescriptionSuppressed = false;
                bool.TryParse(forceSuppressDescription, out isDescriptionSuppressed);

                return fullResults//allResults
                    .Select(o => Tuple.Create(o, schema, isDescriptionSuppressed))
                    .ToTask(state)
                    .ContinueWith(task =>
                    {
                        //task.Result.AddRange(temp);
                        callback(task);
                    });
            }
            catch (Exception exception)
            {
                return Observable
                    .Create<List<FeedResult>>(observer =>
                    {
                        observer.OnError(exception);
                        return Disposable.Empty;
                    })
                    .ToTask(state)
                    .ContinueWith(task => callback(task));
            }
        }

        public Stream EndGetWeave(IAsyncResult result)
        {
            try
            {
                var task = (Task<Tuple<List<FeedResult>, string, bool>>)result;

                if (task.Exception != null)
                {
                    return CreateErrorStream(task.Exception);
                }

                var temp = task.Result;
                var results = temp.Item1;// task.Result;
                var schema = temp.Item2;
                var isDescriptionSuppressed = temp.Item3;

                if (isDescriptionSuppressed && results != null)
                {
                    foreach (var r in results)
                    {
                        if (r.News != null)
                        {
                            foreach (var newsItem in r.News)
                                newsItem.Description = null;
                        }
                    }
                }

                if ("json".Equals(schema, StringComparison.OrdinalIgnoreCase))
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "text/json; charset=UTF-8";
                    WebOperationContext.Current.OutgoingResponse.Headers.Add(
                        HttpResponseHeader.ContentEncoding, "gzip");

                    var serializer = new DataContractJsonSerializer(typeof(List<FeedResult>));
                    return new MemoryStream(Compress(results, serializer.WriteObject));
                }
                else
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
                    WebOperationContext.Current.OutgoingResponse.Headers.Add(
                        HttpResponseHeader.ContentEncoding, "gzip");

                    return new MemoryStream(Compress(results, ProtoBuf.Serializer.Serialize));
                }
            }
            catch (Exception exception)
            {
                return CreateErrorStream(exception);
            }
        }

        Stream CreateErrorStream(Exception exception)
        {
            string error = string.Empty;
            if (exception != null)
                error = string.Format("Error: {0}\r\n\r\n{1}", exception.InnerException, exception.StackTrace);

            byte[] errorBytes = Encoding.UTF8.GetBytes(error);
            return new MemoryStream(errorBytes);
        }

        public static byte[] Compress<T>(T data, Action<Stream, T> serialize)
        {
            byte[] result = null;
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    serialize(zip, data);
                }

                result = memory.ToArray();
            }

            return result;
        }

        public static byte[] Compress<T>(T data)
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            Action<Stream, object> serializer = (s, o) => formatter.Serialize(s, o);
            return Compress(data, serializer);
        }



        byte[] ok = Encoding.UTF8.GetBytes("OK");
        public Stream Ping()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            return new MemoryStream(ok);
        }
    }
}
