using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;
using Weave.User.Service.Redis.Json;

namespace Weave.User.Service.Redis
{
    public class NewsItemCache
    {
        ConnectionMultiplexer connection;
        IDatabase db;

        public NewsItemCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            db = connection.GetDatabase(0);
        }

        public async Task<IEnumerable<RedisCacheResult<NewsItem>>> Get(IEnumerable<Guid> newsItemIds)
        {
            var db = connection.GetDatabase(0);
            var keys = newsItemIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();

            var values = await db.StringGetAsync(keys, CommandFlags.None);
            var results = values.Select(o => o.ReadAs<NewsItem>());
            return results;
        }

        public async Task<IEnumerable<bool>> Set(IEnumerable<NewsItem> newsItems)
        {
            var requests = newsItems.Select(CreateSaveRequest);
            var results = await Task.WhenAll(requests);
            return results;
        }

        Task<bool> CreateSaveRequest(NewsItem newsItem)
        {
            if (newsItem == null)
                return Task.FromResult(false);

            RedisKey key;
            RedisValue value;

            try
            {
                key = (RedisKey)newsItem.Id.ToByteArray();
                //var redisNewsItem = Map(newsItem);
                value = newsItem.WriteAs();
            }
            catch
            {
                return Task.FromResult(false);
            }

            if (!value.HasValue)
                return Task.FromResult(false);

            return db.StringSetAsync(key, value, TimeSpan.FromDays(60), When.NotExists, CommandFlags.None);
        }


        #region Helper functions



        //NewsItem Map(BusinessObjects.NewsItem o)
        //{
        //    return new NewsItem
        //    {
        //        Id = o.Id,
        //        UtcPublishDateTime = o.UtcPublishDateTime,
        //        Title = o.Title,
        //        Link = o.Link,
        //        ImageUrl = o.ImageUrl,
        //        YoutubeId = o.YoutubeId,
        //        VideoUri = o.VideoUri,
        //        PodcastUri = o.PodcastUri,
        //        ZuneAppId = o.ZuneAppId,
        //        Image = o.Image,
        //    };
        //}

        //BusinessObjects.NewsItem Map(NewsItem o)
        //{
        //    return new BusinessObjects.NewsItem
        //    {
        //        Id = o.Id,
        //    };
        //}

        #endregion
    }
}
