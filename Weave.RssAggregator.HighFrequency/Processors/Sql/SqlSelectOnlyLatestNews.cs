using System.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlSelectOnlyLatestNews : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        SqlClient dbClient;

        public SqlSelectOnlyLatestNews(SqlClient dbClient)
        {
            this.dbClient = dbClient;
            IsHandledFully = false;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            var entries = update.Entries;

            var latestNewsItemTimeStamp = await dbClient.GetLatestForFeedId(update.FeedId);

            var latestNews = entries.Where(o => o.UtcPublishDateTime > latestNewsItemTimeStamp).ToList();

            if (!latestNews.Any())
            {
                IsHandledFully = true;
                return;
            }
            else
            {
                update.Entries = latestNews;
            }
        }
    }
}
