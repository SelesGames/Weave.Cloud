using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LowFrequency
{
    public class HFeedDbMediator
    {
        DbClient dbClient;
        CachedFeed feed;

        public Exception CurrentLoadLatestException { get; private set; }

        public HFeedDbMediator(DbClient dbClient, CachedFeed feed)
        {
            if (dbClient == null) throw new ArgumentNullException("dbClient in HFeedDbMediator ctor");
            if (feed == null) throw new ArgumentNullException("feed in HFeedDbMediator ctor");

            this.dbClient = dbClient;
            this.feed = feed;
        }

        public async Task LoadLatestNews()
        {
            CurrentLoadLatestException = null;

            try
            {
                var news = await dbClient.GetLatestNNewsAsBlobList(feed.FeedId);

                // if there is no news, assume something is wrong and leave it in the Uninitialized FeedState
                if (news == null || !news.Any())
                    return;

                var latest = news.First();
                var oldest = news.Last();

                feed.MostRecentNewsItemPubDate = latest.PublishDateTime;
                feed.OldestNewsItemPubDate = oldest.PublishDateTime;
                feed.News = news;

                feed.LastFeedState = CachedFeed.FeedState.OK;
            }
            catch (Exception ex)
            {
                CurrentLoadLatestException = ex;
                feed.LastFeedState = CachedFeed.FeedState.Failed;
            }
        }
    }
}
