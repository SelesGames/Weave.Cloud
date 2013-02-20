using Common.Data;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Sql = RssAggregator.Data.Sql;

namespace Weave.RssAggregator.LowFrequency
{
    public class DbClient
    {
        IProvider<ITransactionalDatabaseClient> dbClientProvider;

        public DbClient(IProvider<ITransactionalDatabaseClient> dbClientProvider)
        {
            this.dbClientProvider = dbClientProvider;
        }

        public async Task<List<NewsItem>> GetLatestNewsForFeedId(Guid feedId)
        {
            using (var client = dbClientProvider.Get())
            {
                var news = await client.Get<Sql.NewsItem, NewsItem>(x => x
                    .Where(o => feedId.Equals(o.Id))
                    .OrderByDescending(o => o.PublishDateTime)
                    .Take(25)
                    .Select(o => new NewsItem
                    {
                        Title = o.Title,
                        Link = o.Link,
                        PublishDateTime = o.PublishDateTimeString,
                        ImageUrl = o.ImageUrl,
                        VideoUri = o.VideoUri,
                        YoutubeId = o.YoutubeId,
                        PodcastUri = o.PodcastUri,
                        ZuneAppId = o.ZuneAppId,
                    }));

                return news.ToList();
            }
        }
    }
}
