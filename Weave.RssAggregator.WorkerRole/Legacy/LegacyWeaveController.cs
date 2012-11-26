using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.Legacy
{
    public class LegacyWeaveController : ApiController
    {
        readonly Weave.RssAggregator.WorkerRole.Controllers.WeaveController underlyingController;

        public LegacyWeaveController(HighFrequencyFeedCache cache)
        {
            this.underlyingController = new Weave.RssAggregator.WorkerRole.Controllers.WeaveController(cache);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Get([FromBody] List<FeedRequest> requests, bool fsd = true)
        {
            var results = await underlyingController.Get(requests, fsd);

            var streamContent = new StreamContent(new MemoryStream(Compress(results, Serializer.Serialize)));
            streamContent.Headers.ContentEncoding.Add("gzip");
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var responseMessage = new HttpResponseMessage { Content = streamContent };
            //responseMessage.Headers.TransferEncodingChunked = true;
            return responseMessage;
        }




        #region helper methods
        
        static byte[] Compress<T>(T data, Action<Stream, T> serialize)
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

        #endregion
    }
}
