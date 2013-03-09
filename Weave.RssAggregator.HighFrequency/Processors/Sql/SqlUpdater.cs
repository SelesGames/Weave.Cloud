using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        SqlClient dbClient;

        public SqlUpdater(SqlClient dbClient)
        {
            this.dbClient = dbClient;
            IsHandledFully = false;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            int successCount = 0;
            foreach (var newsItem in Enumerable.Reverse(update.Entries))
            {
                if (await TryInsertAsync(newsItem))
                    successCount++;
            }
            DebugEx.WriteLine("SqlUpdater processed: {0}, {1} inserted into database", update.FeedUri, successCount);
        }




        #region private helper functions

        async Task<bool> TryInsertAsync(EntryWithPostProcessInfo entry)
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
