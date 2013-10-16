using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
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

                var news = await dbClient.GetLatestNNewsAsBlobList(feed.FeedId, 64);

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

        public async void ProcessFeedUpdateNotice(FeedUpdateNotice notice)
        {
            try
            {
                if (notice == null)
                    return;

                if (notice.FeedId.Equals(feed.FeedId))
                {
                    if (notice.RefreshTime > LastRefresh)
                    {
                        await LoadLatestNews();
                    }

                    DebugEx.WriteLine("completing message id: {0}", notice.MessageId);
                    await notice.MarkNoticeAsRead();
                    DebugEx.WriteLine("COMPLETED message id: {0}", notice.MessageId);
                }
            }
#if DEBUG
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }
#else
            catch { }
#endif
        }
    }
}
