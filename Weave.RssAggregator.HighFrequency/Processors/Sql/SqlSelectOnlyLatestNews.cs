using System.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlSelectOnlyLatestNews : ISequentialAsyncProcessor<HighFrequencyFeedUpdate>
    {
        SqlClient dbClient;

        public SqlSelectOnlyLatestNews(SqlClient dbClient)
        {
            this.dbClient = dbClient;
            IsHandledFully = false;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            var entries = update.InnerUpdate.Entries;

            var latestNewsItemTimeStamp = await dbClient.GetLatestForFeedId(update.Feed.Id);

            var latestNews = entries.Where(o => o.UtcPublishDateTime > latestNewsItemTimeStamp).ToList();

            if (!latestNews.Any())
            {
                IsHandledFully = true;
                DebugEx.WriteLine("SqlSelectOnlyLatestNews processed: {0}, NO NEW NEWS", update.Feed.Uri);
                return;
            }
            else
            {
                int totalCount = update.InnerUpdate.Entries.Count;
                update.InnerUpdate.Entries = latestNews;
                int filteredCount = update.InnerUpdate.Entries.Count;
                DebugEx.WriteLine("SqlSelectOnlyLatestNews processed: {0}, filtered from {1} down to {2} articles", update.Feed.Uri, totalCount, filteredCount);
            }
        }
    }
}