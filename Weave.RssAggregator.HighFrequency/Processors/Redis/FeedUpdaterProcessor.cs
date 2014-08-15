using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    /// <summary>
    /// Saves the state of the Updater Feed
    /// </summary>
    public class FeedUpdaterProcessor : ISequentialAsyncProcessor<FeedUpdate>
    {
        readonly ConnectionMultiplexer connection;

        public FeedUpdaterProcessor(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(FeedUpdate update)
        {
            try
            {
                var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
                var updater = new FeedUpdaterCache(db);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await updater.Save(update.Feed);
                sw.Stop();
                DebugEx.WriteLine("Took {0} ms to save feed {1}", sw.Elapsed.TotalMilliseconds, update.Feed.Name);
                DebugEx.WriteLine(result);
            }
            catch { }
        }
    }
}