using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlSelectOnlyLatestNews : ISequentialAsyncProcessor<FeedUpdate>
    {
        SqlClient dbClient;

        public SqlSelectOnlyLatestNews(SqlClient dbClient)
        {
            this.dbClient = dbClient;
            IsHandledFully = false;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(FeedUpdate update)
        {
            var entries = update.Entries;

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
                int totalCount = update.Entries.Count;
                update.Entries = latestNews;
                int filteredCount = update.Entries.Count;
                DebugEx.WriteLine("SqlSelectOnlyLatestNews processed: {0}, filtered from {1} down to {2} articles", update.Feed.Uri, totalCount, filteredCount);
            }
        }
    }
}
