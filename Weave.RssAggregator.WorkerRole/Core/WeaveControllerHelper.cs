using RssAggregator.IconCaching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.Updater.BusinessObjects;
using Weave.Updater.PubSub;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.WorkerRole
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
            try
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
            catch(Exception ex)
            {
                Metadata.ExceptionMessage = ex.ToString();
                return new Result { IsLoaded = false, Url = url, Meta = Metadata };
            }
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
            DateTime previousFeedUpdate;

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

            // record the timestamp of LastRefreshedOn prior to doing a new refresh
            previousFeedUpdate = feed.LastRefreshedOn;

            sw.Start();
            var update = await feed.Refresh();
            Metadata.Refresh = sw.Record().Dump();

            // if LastRefreshedOn hasn't changed, then the feed wasn't updated
            // maybe switch this to a new ETAG or something?
            if (feed.LastRefreshedOn == previousFeedUpdate)
                return;

            // save the newly refreshed feed state
            var saveUpdaterFeedResult = await cache.Save(feed);
            Metadata.SaveUpdaterFeed_SaveTime = saveUpdaterFeedResult.Timings.ServiceTime.Dump();
            Metadata.SaveUpdaterFeed_SerializationTime = saveUpdaterFeedResult.Timings.SerializationTime.Dump();

            if (update != null && update.Entries.Any())
            {
                // fix images - for when ImageUrls haven't been transferred to Images
                foreach (var entry in update.Entries)
                    FixImages(entry);

                var saveFeedIndexAndNewsIndices = await SaveNewEntries(update.Entries);
                Metadata.SaveNewEntries_SaveTime = saveFeedIndexAndNewsIndices.Timings.ServiceTime.Dump();
                Metadata.SaveNewEntries_SerializationTime = saveFeedIndexAndNewsIndices.Timings.SerializationTime.Dump();
            }

            // Send a notice via Redis PubSub that the feed updater was updated
            var bridge = new FeedUpdateEventBridge(connection);
            var received = await bridge.Publish(new FeedUpdateNotice { FeedUri = url, RefreshTime = feed.LastRefreshedOn });
            Metadata.NumberThatReceivedPubSubNotification = received;
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

            var saveResultTask = cache.Set(entries, overwrite: false);
            await transaction.ExecuteAsync(flags: CommandFlags.None);
            return await saveResultTask;
        }
    }
}