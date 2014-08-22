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

        public async Task<Result> GetResultFromRequest(string url)
        {
            VerifyRequest(url);

            Result result = null;

            Metadata.Url = url;

            sw.Start();
            var cachedFeed = feedCache.Get(url);
            Metadata.CheckIfFeedIsHighFrequencyCached = sw.Record().Dump();

            if (cachedFeed != null)
            {
                // don't need to do a refresh, since it was cached and refreshes on its own timer
                var key = (RedisKey)url;
                var isFeedIndexLoaded = await CheckIfFeedUpdaterExists(key);

                if (!isFeedIndexLoaded)
                {
                    await RefreshFeed(url);
                }
                result = new Result { IsLoaded = true, };
            }
            else
            {
                await RefreshFeed(url);
                result = new Result { IsLoaded = true, };
            }

            result.Meta = Metadata;
            return result;
        }

        static void VerifyRequest(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("INVALID URL: request in WeaveControllerHelper");
        }

        async Task<bool> CheckIfFeedUpdaterExists(RedisKey key)
        {
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);

            sw.Start();
            var isFeedIndexLoaded = await db.KeyExistsAsync(key, flags: CommandFlags.None);
            Metadata.IsFeedUpdaterPresent = sw.Record().Dump();

            return isFeedIndexLoaded;
        }

        async Task RefreshFeed(string url)
        {
            // create the feed we will use for doing the actual refresh here
            var feed = new Feed(url);
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);

            // the feed's prior data may or may not exist in Redis
            sw.Start();
            var existingFeedResult = await cache.Get(feed.Uri);
            if (existingFeedResult.HasValue)
            {
                var existingFeed = existingFeedResult.Value;
                existingFeed.CopyStateTo(feed);
            }
            Metadata.RecoverUpdaterFeed = sw.Record().Dump();

            sw.Start();
            var update = await feed.Refresh();
            Metadata.Refresh = sw.Record().Dump();

            // save the newly refreshed feed state
            var saveUpdaterFeedResult = await cache.Save(feed);
            Metadata.SaveUpdaterFeed_SaveTime = saveUpdaterFeedResult.Timings.ServiceTime;
            Metadata.SaveUpdaterFeed_SerializationTime = saveUpdaterFeedResult.Timings.SerializationTime;

            if (update != null && update.Entries.Any())
            {
                // fix images - for when ImageUrls haven't been transferred to Images
                foreach (var entry in update.Entries)
                    FixImages(entry);

                var saveFeedIndexAndNewsIndices = await SaveNewEntries(update.Entries);
                Metadata.SaveNewEntries_SaveTime = saveFeedIndexAndNewsIndices.Timings.ServiceTime;
                Metadata.SaveNewEntries_SerializationTime = saveFeedIndexAndNewsIndices.Timings.SerializationTime;
            }
        }

        static void FixImages(ExpandedEntry entry)
        {
            if (!entry.Images.Any() && entry.ImageUrls.Any())
            {
                foreach (var imageUrl in entry.ImageUrls)
                    entry.Images.Add(new Image { Url = imageUrl });
            }
        }

        async Task<RedisWriteMultiResult<bool>> SaveNewEntries(IEnumerable<ExpandedEntry> entries)
        {
            var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var transaction = db.CreateTransaction();
            var cache = new ExpandedEntryCache(transaction);

            var saveResultTask = cache.Set(entries);
            await transaction.ExecuteAsync(flags: CommandFlags.None);
            return await saveResultTask;
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
            return GetResultFromRequest(feedUri);
        }

        [HttpPost]
        public async Task<List<Result>> Get([FromBody] List<string> uris, [FromUri] bool fsd = true)
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
            var helper = new WeaveControllerHelper(feedCache, iconCache, connection);
            return helper.GetResultFromRequest(uri);
        }
    }

    static class TimeSpanFormattingExtensions
    {
        public static string Dump(this TimeSpan t)
        {
            return t.TotalMilliseconds + " ms";
        }
    }
}
