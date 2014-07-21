using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    public class UserIndexCache
    {
        readonly ConnectionMultiplexer connection;
        readonly RedisValueSerializer serializer;

        public UserIndexCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            //serializer = new RedisValueSerializer(new ProtobufSerializerHelper());
            serializer = new RedisValueSerializer(new UserIndexBinarySerializer());
        }

        public async Task<RedisCacheResult<UserIndex>> Get(Guid userId)
        {
            var db = connection.GetDatabase(0);
            var key = (RedisKey)userId.ToByteArray();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var value = await db.StringGetAsync(key, CommandFlags.None);
            sw.Stop();
            DebugEx.WriteLine("the actual getting of the user index took {0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var cacheResult = serializer.ReadAs<UserIndex>(value);
            sw.Stop();
            DebugEx.WriteLine("deserializing the user index took {0} ms", sw.ElapsedMilliseconds);

            return cacheResult;

            //if (cacheResult.Value != null)
            //{
            //    sw.Restart();
            //    var store = Map(cacheResult.Value);
            //    sw.Stop();
            //    DebugEx.WriteLine("converting the user index took {0} ms", sw.ElapsedMilliseconds);

            //    var result = RedisCacheResult.Create(store, cacheResult.RedisValue);
            //    return result;
            //}
            //else
            //{
            //    return RedisCacheResult.Create(default(UserIndex), cacheResult.RedisValue);
            //}
        }

        public Task<bool> Save(UserIndex userIndex)
        {
            var db = connection.GetDatabase(0);
            var key = (RedisKey)userIndex.Id.ToByteArray();
            //var store = Map(userIndex);
            var val = serializer.WriteAs(userIndex);//store);

            return db.StringSetAsync(key, val, TimeSpan.FromDays(7), When.Always, CommandFlags.HighPriority);
        }




        #region Map functions

        //UserIndex Map(DTOs.UserIndex o)
        //{
        //    var userIndex = new UserIndex
        //    {
        //        Id = o.Id,
        //        PreviousLoginTime = o.PreviousLoginTime,
        //        CurrentLoginTime = o.CurrentLoginTime,
        //        ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
        //        ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
        //    };

        //    if (o.FeedIndices != null)
        //    {
        //        foreach (var feedIndex in o.FeedIndices.Select(Map))
        //        {
        //            userIndex.FeedIndices.Add(feedIndex);
        //        }
        //    }

        //    return userIndex;
        //}

        //FeedIndex Map(DTOs.FeedIndex o)
        //{
        //    var feedIndex = new FeedIndex
        //    {
        //        Id = o.Id,
        //        Uri = o.Uri,
        //        Name = o.Name,
        //        IconUri = o.IconUri,
        //        Category = o.Category,
        //        TeaserImageUrl = o.TeaserImageUrl,
        //        ArticleViewingType = o.ArticleViewingType,
        //        LastRefreshedOn = o.LastRefreshedOn,
        //        Etag = o.Etag,
        //        LastModified = o.LastModified,
        //        MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
        //        MostRecentEntrance = o.MostRecentEntrance,
        //        PreviousEntrance = o.PreviousEntrance,
        //    };

        //    if (o.NewsItemIndices != null)
        //    {
        //        foreach (var newsItemIndex in o.NewsItemIndices)
        //        {
        //            feedIndex.NewsItemIndices.Add(newsItemIndex);
        //        }
        //    }

        //    return feedIndex;
        //}

        //DTOs.UserIndex Map(UserIndex o)
        //{
        //    return new DTOs.UserIndex
        //    {
        //        Id = o.Id,
        //        PreviousLoginTime = o.PreviousLoginTime,
        //        CurrentLoginTime = o.CurrentLoginTime,
        //        ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
        //        ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
        //        FeedIndices = o.FeedIndices.Select(Map).ToList(),
        //    };
        //}

        //DTOs.FeedIndex Map(FeedIndex o)
        //{
        //    return new DTOs.FeedIndex
        //    {
        //        Id = o.Id,
        //        Uri = o.Uri,
        //        Name = o.Name, 
        //        IconUri = o.IconUri,
        //        Category = o.Category,
        //        TeaserImageUrl = o.TeaserImageUrl,
        //        ArticleViewingType = o.ArticleViewingType,
        //        LastRefreshedOn = o.LastRefreshedOn,
        //        Etag = o.Etag,
        //        LastModified = o.LastModified,
        //        MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
        //        MostRecentEntrance = o.MostRecentEntrance,
        //        PreviousEntrance = o.PreviousEntrance,
        //        NewsItemIndices = o.NewsItemIndices.ToList(),
        //    };
        //}

        #endregion
    }
}
