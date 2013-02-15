using Common.Data;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Parsing;
using Sql = RssAggregator.Data.Sql;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlUpdater
    {
        IObservable<Tuple<HighFrequencyFeed, List<Entry>>> updateQueue;
        SqlClient dbClient;
        IProvider<ITransactionalDatabaseClient> transactClientProvider;

        public SqlUpdater(SqlClient dbClient, IProvider<ITransactionalDatabaseClient> transactClientProvider)
        {
            this.dbClient = dbClient;
            this.transactClientProvider = transactClientProvider;
        }

        public void Register(HighFrequencyFeed feed)
        {
            var feedUpdate = feed.FeedUpdate.Select(o => Tuple.Create(feed, o));

            if (updateQueue == null)
                updateQueue = feedUpdate;
            else
                updateQueue = updateQueue.Merge(feedUpdate);

            Resubscribe();
        }

        void Resubscribe()
        {
            if (updateQueue == null)
                return;

            updateQueue.Subscribe(
                SafeOnHfFeedUpdate,
                exception =>
                {
                    DebugEx.WriteLine(exception);
                    Resubscribe();
                });
        }

        async void SafeOnHfFeedUpdate(Tuple<HighFrequencyFeed, List<Entry>> update)
        {
            try
            {
                await OnHfFeedUpdate(update);
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }
        }


        async Task OnHfFeedUpdate(Tuple<HighFrequencyFeed, List<Entry>> feed)
        {
            var latest = await dbClient.GetLatestForFeedId(feed.Item1.FeedId);
            var latestNews = feed.Item2.Where(o => o.PublishDateTime > latest).Select(Translate);
            if (!latestNews.Any())
                return;

            foreach (var newsItem in latestNews)
            {
                using (var transactClient = transactClientProvider.Get())
                {
                    transactClient.Insert(newsItem);
                    await transactClient.SubmitChanges();
                }
            }
        }

        Sql.NewsItem Translate(Entry e)
        {
            return new Sql.NewsItem
            {
                Id = e.Id,
                FeedId = e.FeedId,
                Title = e.Title,
                Link = e.Link,
                Description = e.Description,
                PublishDateTime = e.PublishDateTime,
                PublishDateTimeString = e.PublishDateTimeString,
                ImageUrl = e.ImageUrl,
                VideoUri = e.VideoUri,
                YoutubeId = e.YoutubeId,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                OriginalRssXml = e.OriginalRssXml,
                //NewsItemBlob = ConvertToBytes
            };
        }
    }
}
