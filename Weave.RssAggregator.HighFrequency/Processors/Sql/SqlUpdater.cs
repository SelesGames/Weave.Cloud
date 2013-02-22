using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Client;

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
            foreach (var newsItem in Enumerable.Reverse(update.Entries))
            {
                await TryInsertAsync(newsItem);
            }
        }




        #region private helper functions

        async Task<bool> TryInsertAsync(Entry entry)
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
