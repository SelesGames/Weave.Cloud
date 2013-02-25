using Microsoft.ServiceBus.Messaging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LowFrequency
{
    public class HFeedDbMediator
    {
        DbClient dbClient;
        CachedFeed feed;

        public DateTime LastRefresh { get; private set; }
        public Exception CurrentLoadLatestException { get; private set; }

        public HFeedDbMediator(DbClient dbClient, CachedFeed feed)
        {
            if (dbClient == null) throw new ArgumentNullException("dbClient in HFeedDbMediator ctor");
            if (feed == null) throw new ArgumentNullException("feed in HFeedDbMediator ctor");

            this.dbClient = dbClient;
            this.feed = feed;

            LastRefresh = DateTime.MinValue;
        }

        public async Task LoadLatestNews()
        {
            CurrentLoadLatestException = null;

            try
            {
                var now = DateTime.UtcNow;

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

                LastRefresh = now;
            }
            catch (Exception ex)
            {
                CurrentLoadLatestException = ex;
                feed.LastFeedState = CachedFeed.FeedState.Failed;
            }
        }

        public void Subscribe(IObservable<BrokeredMessage> observable)
        {
            observable
                .Where(m => m.Properties.ContainsKey("FeedId") && m.Properties["FeedId"].Equals(feed.FeedId))
                .Where(m => m.Properties.ContainsKey("RefreshTime") && ((DateTime)m.Properties["RefreshTime"]) > LastRefresh)
                .Subscribe(_ => LoadLatestNews());
        }
    }
}
