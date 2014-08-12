using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlUpdater : ISequentialAsyncProcessor<FeedUpdate>
    {
        SqlClient dbClient;

        public SqlUpdater(SqlClient dbClient)
        {
            this.dbClient = dbClient;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(FeedUpdate update)
        {
            int successCount = 0;
            foreach (var newsItem in Enumerable.Reverse(update.Entries))
            {
                if (await TryInsertAsync(newsItem))
                    successCount++;
            }
            DebugEx.WriteLine("SqlUpdater processed: {0}, {1} inserted into database", update.Feed.Uri, successCount);
        }




        #region private helper functions

        async Task<bool> TryInsertAsync(ExpandedEntry entry)
        {
            try
            {
                await dbClient.InsertNewsItemIfNotExists(entry);
                return true;
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                return false;
            }
        }

        #endregion
    }
}
