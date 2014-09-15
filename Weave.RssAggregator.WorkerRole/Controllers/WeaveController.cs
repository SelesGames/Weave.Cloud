﻿using RssAggregator.IconCaching;
using SelesGames.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.WorkerRole.Controllers
{
    public class WeaveController : ApiController
    {
        readonly FeedCache feedCache;
        readonly NLevelIconUrlCache iconCache;

        public WeaveController(FeedCache feedCache, NLevelIconUrlCache iconCache)
        {
            this.feedCache = feedCache;
            this.iconCache = iconCache;
        }

        [HttpGet]
        public Task<Result> Get([FromUri] string feedUri)
        {
            return GetResultFromRequest(feedUri);
        }

        [HttpPost]
        public async Task<List<Result>> Get([FromBody] List<string> uris)
        {
            if (uris == null || !uris.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one string url in the message body");

            var temp = await Task.WhenAll(uris.Select(GetResultFromRequest));
            var results = temp.ToList();
            return results;
        }

        Task<Result> GetResultFromRequest(string uri)
        {
            var helper = new WeaveControllerHelper(feedCache, iconCache);
            return helper.GetResultFromRequest(uri);
        }
    }
}