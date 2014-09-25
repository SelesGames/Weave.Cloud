using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Clients;

namespace Weave.FeedUpdater.HighFrequency
{
    /// <summary>
    /// Saves the state of the Updater Feed
    /// </summary>
    public class FeedUpdaterProcessor : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly ConnectionMultiplexer connection;

        public FeedUpdaterProcessor()
        {
            this.connection = Settings.StandardConnection;
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            var saveFeedResult = await SaveFeed(update.InnerFeed);
            DebugEx.WriteLine("** FEED UPDATER PROCESSOR ** Took {0} ms to serialize, {1} ms to save updater feed for {2}", saveFeedResult.Timings.SerializationTime.TotalMilliseconds, saveFeedResult.Timings.ServiceTime.TotalMilliseconds, update.Feed.Name);

            foreach (var previous in update.Feed.PreviousUris)
            {
                var feed = new Feed(previous);
                update.InnerFeed.CopyStateTo(feed);
                await SaveFeed(feed);
            }

            if (update.Entries.Any())
            {
                var saveNewsResults = await SaveNewNews(update);
                DebugEx.WriteLine("** FEED UPDATER PROCESSOR ** Blob save: {0}, Redis save: {1} *** for {2}", saveNewsResults.Meta.BlobSave, saveNewsResults.Meta.RedisSave, update.Feed.Name);
            }
        }




        #region Redis Write functions

        Task<RedisWriteResult<bool>> SaveFeed(Feed feed)
        {
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);
            return cache.Save(feed);
        }

        Task<CacheSaveResult> SaveNewNews(HighFrequencyFeedUpdate update)
        {
            var saveHelper = ExpandedEntryCacheFactory.CreateSaveHelper();
            return saveHelper.Save(update.Entries, overWrite: true);
        }

        #endregion
    }
}