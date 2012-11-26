using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.HighFrequency;
using Weave.RssAggregator.WorkerRole.Controllers;


namespace Weave.RssAggregator.WorkerRole.Legacy
{
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = false,
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple
    )]
    public class RssAggregator : IRssAggregator, IPing
    {
        readonly WeaveController underlyingController;

        public RssAggregator(HighFrequencyFeedCache cache)
        {
            this.underlyingController = new WeaveController(cache);
        }

        public async Task<Stream> Get(Stream feedRequests, string forceSuppressDescription)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(List<FeedRequest>));
            var requests = (List<FeedRequest>)jsonSerializer.ReadObject(feedRequests);

            var results = await underlyingController.Get(requests, bool.Parse(forceSuppressDescription));

            WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
            WebOperationContext.Current.OutgoingResponse.Headers.Add(
                HttpResponseHeader.ContentEncoding, "gzip");

            return new MemoryStream(Compress(results, ProtoBuf.Serializer.Serialize));
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

        byte[] ok = Encoding.UTF8.GetBytes("OK");
        public Stream Ping()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            return new MemoryStream(ok);
        }
    }
}
