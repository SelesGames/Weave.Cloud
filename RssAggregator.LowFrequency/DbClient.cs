using Common.Data;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Sql = RssAggregator.Data.Sql;
using ProtoBuf;
using System.IO;

namespace Weave.RssAggregator.LowFrequency
{
    public class DbClient
    {
        IProvider<ITransactionalDatabaseClient> dbClientProvider;

        static DbClient()
        {
            ProtoBuf.Serializer.PrepareSerializer<NewsItem>();
        }

        public DbClient(IProvider<ITransactionalDatabaseClient> dbClientProvider)
        {
            this.dbClientProvider = dbClientProvider;
        }

        public async Task<List<NewsItem>> GetLatestNewsForFeedId(Guid feedId)
        {
            using (var client = dbClientProvider.Get())
            {
                var news = await client.Get<Sql.NewsItem, NewsItem>(x => x
                    .Where(o => feedId.Equals(o.FeedId))
                    .OrderByDescending(o => o.PublishDateTime)
                    .Take(25)
                    .Select(o => new NewsItem
                    {
                        Title = o.Title,
                        Link = o.Link,
                        PublishDateTime = o.UtcPublishDateTimeString,
                        ImageUrl = o.ImageUrl,
                        VideoUri = o.VideoUri,
                        YoutubeId = o.YoutubeId,
                        PodcastUri = o.PodcastUri,
                        ZuneAppId = o.ZuneAppId,
                    }));

                return news.ToList();
            }
        }

        public async Task<List<NewsItem>> GetLatestNewsForFeedIdViaBinary(Guid feedId)
        {
            using (var client = dbClientProvider.Get())
            {
                var news = await client.Get<Sql.NewsItem, System.Data.Linq.Binary>(x => x
                    .Where(o => feedId.Equals(o.FeedId))
                    .OrderByDescending(o => o.PublishDateTime)
                    .Take(25)
                    .Select(o => o.NewsItemBlob));

                var binaries = news.ToList();
                var byteArrays = binaries.Select(o => o.ToArray());
                var result = byteArrays.Select(FromByteArray).ToList();

                return result;
            }
        }

        NewsItem FromByteArray(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                ms.Position = 0;
                return Serializer.Deserialize<NewsItem>(ms);
            }
        }
    }
}
