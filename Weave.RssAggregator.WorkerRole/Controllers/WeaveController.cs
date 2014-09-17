using RssAggregator.IconCaching;
using SelesGames.WebApi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;

namespace Weave.FeedUpdater.Service.Role.Controllers
{
    public class WeaveController : ApiController
    {
        readonly FeedCache feedCache;
        readonly NLevelIconUrlCache iconCache;

        Task<bool[]> keyCheck;

        public WeaveController(FeedCache feedCache, NLevelIconUrlCache iconCache)
        {
            this.feedCache = feedCache;
            this.iconCache = iconCache;
        }

        [HttpGet]
        public async Task<Result> Get([FromUri] string feedUri)
        {
            var result = await Get(new List<string> { feedUri });
            return result.First();
        }

        [HttpPost]
        public async Task<List<Result>> Get([FromBody] List<string> uris)
        {
            if (uris == null || !uris.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one string url in the message body");

            keyCheck = CreateKeyCheck(uris);

            var temp = await Task.WhenAll(uris.Select(GetResultFromRequest));
            var results = temp.ToList();
            return results;
        }

        Task<Result> GetResultFromRequest(string uri, int index)
        {
            Func<string, Task<bool>> checkFunc = async _ =>
            {
                var results = await keyCheck;
                return results[index];
            };
            var helper = new WeaveControllerHelper(feedCache, iconCache, checkFunc);
            return helper.GetResultFromRequest(uri);
        }

        static async Task<bool[]> CreateKeyCheck(IEnumerable<string> keys)
        {
            var connection = Settings.StandardConnection;
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var batch = db.CreateBatch();

            var ops = new List<Task<bool>>();
            
            foreach (var key in keys)
                ops.Add(batch.KeyExistsAsync(key, CommandFlags.None));

            batch.Execute();

            var results = await Task.WhenAll(ops);
            return results;
        }
    }
}