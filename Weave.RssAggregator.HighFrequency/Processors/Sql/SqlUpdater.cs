using Common.Data;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Parsing;
using Sql = RssAggregator.Data.Sql;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlUpdater : ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>
    {
        SqlClient dbClient;
        IProvider<ITransactionalDatabaseClient> transactClientProvider;

        public SqlUpdater(SqlClient dbClient, IProvider<ITransactionalDatabaseClient> transactClientProvider)
        {
            this.dbClient = dbClient;
            this.transactClientProvider = transactClientProvider;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(Tuple<HighFrequencyFeed, List<Entry>> tup)
        {
            var feed = tup.Item1;
            var entries = tup.Item2;

            var latestNewsItemTimeStamp = await dbClient.GetLatestForFeedId(feed.FeedId);
            var latestNews = entries.Where(o => o.PublishDateTime > latestNewsItemTimeStamp).Select(Translate);

            if (!latestNews.Any())
            {
                IsHandledFully = true;
                return;
            }

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
