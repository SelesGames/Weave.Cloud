using SelesGames.WebApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace WebServiceAndCacheRedirect.WorkerRole.Controllers
{
    public class WeaveController : ApiController
    {
        string location = "http://weave2.cloudapp.net/api";

        public Task<FeedResult> Get(string feedUri)
        {
            var pathAndQuery = Request.RequestUri.PathAndQuery;
            var movedTo = location + pathAndQuery;
            var ex = ResponseHelper.CreateResponseException(HttpStatusCode.MovedPermanently, null);
            ex.Response.Headers.Location = new Uri(movedTo, UriKind.Absolute);
            throw ex;
        }

        [HttpPost]
        public Task<List<FeedResult>> Get([FromBody] List<FeedRequest> requests, bool fsd = true)
        {
            var pathAndQuery = Request.RequestUri.PathAndQuery;
            var movedTo = location + pathAndQuery;
            var ex = ResponseHelper.CreateResponseException(HttpStatusCode.RedirectKeepVerb, null);
            ex.Response.Headers.Location = new Uri(movedTo, UriKind.Absolute);
            throw ex;
        }

        //public Task<FeedResult> GetResultFromRequest(FeedRequest feedRequest, bool fsd = true)
        //{
        //    var pathAndQuery = Request.RequestUri.PathAndQuery;
        //    var movedTo = location + pathAndQuery;
        //    var ex = ResponseHelper.CreateResponseException(HttpStatusCode.MovedPermanently, null);
        //    ex.Response.Headers.Location = new Uri(movedTo, UriKind.Absolute);
        //    throw ex;
        //}
    }
}
