using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    /// <summary>
    /// Saves the state of the Updater Feed
    /// </summary>
    public class FeedUpdaterProcessor : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly ConnectionMultiplexer connection;

        public FeedUpdaterProcessor(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            try
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
                    DebugEx.WriteLine("** FEED UPDATER PROCESSOR ** Took {0} ms to serialize, {1} ms to save news for {2}", saveNewsResults.Timings.SerializationTime.TotalMilliseconds, saveNewsResults.Timings.ServiceTime.TotalMilliseconds, update.Feed.Name);
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("\r\n\r\n**** FeedUpdaterProcessor ERROR ****");
                DebugEx.WriteLine(ex);
            }
        }




        #region Redis Write functions

        Task<RedisWriteResult<bool>> SaveFeed(Feed feed)
        {
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);
            return cache.Save(feed);
        }

        async Task<RedisWriteMultiResult<bool>> SaveNewNews(HighFrequencyFeedUpdate update)
        {
            var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var batch = db.CreateBatch();
            var entryCache = new ExpandedEntryCache(batch);

            var resultTask = entryCache.Set(update.Entries, overwrite: true);

            batch.Execute();

            var result = await resultTask;
            return result;
        }

        #endregion
    }
}