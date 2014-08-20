using RssAggregator.IconCaching;
using SelesGames.WebApi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.LowFrequency;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.WorkerRole.Controllers
{
    public class WeaveControllerHelper
    {
        readonly FeedCache feedCache;
        readonly NLevelIconUrlCache iconCache;
        readonly ConnectionMultiplexer connection;
        readonly dynamic Metadata;
        readonly TimingHelper sw;

        public WeaveControllerHelper(FeedCache cache, NLevelIconUrlCache iconCache, ConnectionMultiplexer connection)
        {
            this.feedCache = cache;
            this.iconCache = iconCache;
            this.connection = connection;
            this.Metadata = new ExpandoObject();
            this.sw = new TimingHelper();
        }

        public async Task<Result> GetResultFromRequest(Request request)
        {
            VerifyRequest(request);

            Result result = null;

            sw.Start();
            var cachedFeed = feedCache.Get(request.Url);
            Metadata.CheckIfFeedIsHighFrequencyCached = sw.Record().Dump();

            if (cachedFeed != null)
            {
                // don't need to do a refresh, since it was cached and refreshes on its own timer
                var key = GetKeyFromRequest(request);
                var isFeedIndexLoaded = await CheckIfCanonicalFeedIndexKeyExists(key);

                if (!isFeedIndexLoaded)
                {
                    await RefreshFeed(request);
                }
                result = new Result { IsLoaded = true, };
            }
            else
            {
                await RefreshFeed(request);
                result = new Result { IsLoaded = true, };
            }

            result.Meta = Metadata;
            return result;
        }

        void VerifyRequest(Request request)
        {
            if (request == null || !Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
                throw new ArgumentException("request in WeaveControllerHelper");
        }

        async Task<bool> CheckIfCanonicalFeedIndexKeyExists(byte[] key)
        {
            var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_FEEDS_AND_NEWSITEMS);

            sw.Start();
            var isFeedIndexLoaded = await db.KeyExistsAsync(key, flags: CommandFlags.None);
            Metadata.IsCanonicalFeedIndexPresent = sw.Record().Dump();

            return isFeedIndexLoaded;
        }

        async Task RefreshFeed(Request request)
        {
            // do some crap to load the feed here
            // create the feed we will use for doing the actual refresh here
            var feed = CreateFeed(request);
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);

            // the feed's prior data may or may not exist in Redis
            sw.Start();
            var wasExistingFeedPresent = await feed.RecoverStateFromRedis(db);
            Metadata.RecoverUpdaterFeed = sw.Record().Dump();

            sw.Start();
            var update = await feed.Refresh();
            Metadata.Refresh = sw.Record().Dump();

            // save the newly refreshed feed state
            var feedUpdater = new FeedUpdaterCache(db);
            sw.Start();
            var saveUpdaterFeedResult = await feedUpdater.Save(feed);
            Metadata.SaveUpdaterFeed = sw.Record().Dump();

            if (update != null)
            {
                sw.Start();
                var saveFeedIndexAndNewsIndices = await SaveFeedandNewsIndices(update);
                Metadata.SaveFeedIndexAndNewsIndices = sw.Record().Dump();
            }
        }

        async Task<SaveFeedIndexAndNewNewsResult> SaveFeedandNewsIndices(FeedUpdate update)
        {
            var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_FEEDS_AND_NEWSITEMS);
            var transaction = db.CreateTransaction();

            var saveResultTask = transaction.SaveFeedIndexAndNewNews(update);
            await transaction.ExecuteAsync(flags: CommandFlags.None);
            var saveResult = await saveResultTask;

            DebugEx.WriteLine("{0}", saveResult);
            return saveResult;
        }

        static Feed CreateFeed(Request o)
        {
            return new Feed("notused", o.Url, null, null)
            {
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                Etag = o.Etag,
                LastModified = o.LastModified,
            };
        }

        static byte[] GetKeyFromRequest(Request request)
        {
            var key = Encoding.UTF8.GetBytes(request.Url);
            return key;

            //Guid id;
            //if (!string.IsNullOrEmpty(request.Id) && Guid.TryParse(request.Id, out id))
            //{
            //    return id.ToByteArray();
            //}
            //else
            //{
            //    var feed = new Feed("unused", request.Url, null, null);
            //    return feed.Id.ToByteArray();
            //}
        }
    }

    public class WeaveController : ApiController
    {
        readonly FeedCache feedCache;
        readonly NLevelIconUrlCache iconCache;
        readonly ConnectionMultiplexer connection;

        public WeaveController(FeedCache feedCache, NLevelIconUrlCache iconCache, ConnectionMultiplexer connection)
        {
            this.feedCache = feedCache;
            this.iconCache = iconCache;
            this.connection = connection;
        }

        [HttpGet]
        public Task<Result> Get([FromUri] string feedUri)
        {
            var feedRequest = new Request { Url = feedUri };
            return GetResultFromRequest(feedRequest);
        }

        [HttpPost]
        public async Task<List<Result>> Get([FromBody] List<Request> requests, [FromUri] bool fsd = true)
        {
            if (requests == null || !requests.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one FeedRequest object in the message body");

            var temp = await Task.WhenAll(requests.Select(GetResultFromRequest));
            var results = temp.ToList();
            return results;
        }




        #region Helper method containing the logic for the get operation

        Task<Result> GetResultFromRequest(Request feedRequest)
        {
            var helper = new WeaveControllerHelper(feedCache, iconCache, connection);
            return helper.GetResultFromRequest(feedRequest);
        }

        #endregion
    }

    static class TimeSpanFormattingExtensions
    {
        public static string Dump(this TimeSpan t)
        {
            return t.TotalMilliseconds + " ms";
        }
    }
}
