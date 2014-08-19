using RssAggregator.IconCaching;
using SelesGames.WebApi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.LowFrequency;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.WorkerRole.Controllers
{
    public class WeaveController : ApiController
    {
        readonly FeedCache feedCache;
        readonly NLevelIconUrlCache iconCache;
        readonly ConnectionMultiplexer connection;
        readonly dynamic Metadata;
        System.Diagnostics.Stopwatch sw;

        public WeaveController(FeedCache cache, NLevelIconUrlCache iconCache, ConnectionMultiplexer connection)
        {
            this.feedCache = cache;
            this.iconCache = iconCache;
            this.connection = connection;
            this.Metadata = new ExpandoObject();
        }

        [HttpGet]
        public Task<Result> Get([FromUri] string feedUri)
        {
            var feedRequest = new FeedRequest { Url = feedUri };
            return GetResultFromRequest(feedRequest);
        }

        [HttpPost]
        public async Task<List<Result>> Get([FromBody] List<FeedRequest> requests, [FromUri] bool fsd = true)
        {
            if (requests == null || !requests.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one FeedRequest object in the message body");

            var temp = await Task.WhenAll(requests.Select(o => GetResultFromRequest(o, fsd)));
            var results = temp.ToList();
            return results;
        }




        #region Helper method containing the logic for the get operation

        async Task<Result> GetResultFromRequest(FeedRequest feedRequest, bool fsd = true)
        {
            Result result = null;

            sw = System.Diagnostics.Stopwatch.StartNew();
            var cachedFeed = feedCache.Get(feedRequest.Url);
            sw.Stop();
            Metadata.CheckIfFeedIsHighFrequencyCached = sw.Elapsed.Dump();

            if (cachedFeed != null)
            {
                // don't need to do a refresh, since it was cached and refreshes on its own timer
                var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_FEEDS_AND_NEWSITEMS);
                var key = GetKeyFromRequest(feedRequest);

                sw.Restart();
                var isFeedIndexLoaded = await db.KeyExistsAsync(key, flags: CommandFlags.None);
                sw.Stop();
                Metadata.IsCanonicalFeedIndexPresent = sw.Elapsed.Dump();

                if (!isFeedIndexLoaded)
                {
                    await RefreshFeed(feedRequest);
                }
                result = new Result { IsLoaded = true, };
            }
            else
            {
                await RefreshFeed(feedRequest);
                result = new Result { IsLoaded = true, };
            }

            result.Meta = Metadata;
            return result;
        }

        async Task RefreshFeed(FeedRequest request)
        {
            // do some crap to load the feed here
            // create the feed we will use for doing the actual refresh here
            var feed = CreateFeed(request);
            var db2 = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);

            // the feed's prior data may or may not exist in Redis
            sw.Restart();
            var wasExistingFeedPresent = await feed.RecoverStateFromRedis(db2);
            sw.Stop();
            Metadata.RecoverUpdaterFeed = sw.Elapsed.Dump();

            sw.Restart();
            var update = await feed.Refresh();
            sw.Stop();
            Metadata.Refresh = sw.Elapsed.Dump();

            // save the newly refreshed feed state
            var feedUpdater = new FeedUpdaterCache(db2);

            sw.Restart();
            var saveUpdaterFeedResult = await feedUpdater.Save(feed);
            sw.Stop();
            Metadata.SaveUpdaterFeed = sw.Elapsed.Dump();

            if (update != null)
            {
                sw.Restart();
                var saveFeedIndexAndNewsIndices = await SaveFeedandNewsIndices(update);
                sw.Stop();
                Metadata.SaveFeedIndexAndNewsIndices = sw.Elapsed.Dump();
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

        static Feed CreateFeed(FeedRequest o)
        {
            return new Feed("notused", o.Url, null, null)
            {
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                Etag = o.Etag,
                LastModified = o.LastModified,
            };
        }

        static byte[] GetKeyFromRequest(FeedRequest request)
        {
            Guid id;
            if (!string.IsNullOrEmpty(request.Id) && Guid.TryParse(request.Id, out id))
            {
                return id.ToByteArray();
            }
            else
            {
                var feed = new Feed("unused", request.Url, null, null);
                return feed.Id.ToByteArray();
            }
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
