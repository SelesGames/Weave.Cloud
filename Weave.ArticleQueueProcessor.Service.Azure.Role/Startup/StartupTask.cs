using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Article.Service.DTOs;
using Weave.Article.Service.DTOs.ServerIncoming;
using Weave.User.Service.InterRoleMessaging.Articles;
using Weave.User.Service.Redis;

namespace Weave.User.Service.Role.Startup
{
    internal class StartupTask
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        readonly TimeSpan pollingInterval = TimeSpan.FromMilliseconds(30);
        readonly Article.Service.Client.ServiceClient articleService;
        readonly ArticleStateChangeMessageQueue messageQueue;
        readonly NewsItemCache newsCache;

        public StartupTask()
        {
            articleService = new Article.Service.Client.ServiceClient();

            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);

            messageQueue = new ArticleStateChangeMessageQueue(connectionMultiplexer);
            newsCache = new NewsItemCache(connectionMultiplexer);
        }

        public void OnStart()
        {
            BeginPolling();
        }

        async void BeginPolling()
        {
            while (true)
            {
                await ProcessNext();
                await Task.Delay(pollingInterval);
            }
        }

        async Task ProcessNext()
        {
            var message = await messageQueue.GetNext();
            if (message == null)
                return;

            var change = message.Value.Change;
            var userId = message.Value.UserId;
            var articleId = message.Value.ArticleId;

            if (change == ArticleStateChange.Read)
                await MarkRead(userId, articleId);

            else if (change == ArticleStateChange.Unread)
                await MarkUnread(userId, articleId);

            else if (change == ArticleStateChange.Favorite)
                await Favorite(userId, articleId);

            else if (change == ArticleStateChange.Unfavorite)
                await Unfavorite(userId, articleId);
        }




        #region Find the News Item - either from the Redis cache, or the user object graph

        async Task<SavedNewsItem> FindNewsItem(Guid userId, Guid articleId)
        {
            var cacheResults = await newsCache.Get(new[] { articleId });
            var cacheResult = cacheResults.FirstOrDefault();

            if (cacheResult != null && cacheResult.HasValue)
            {
                var newsItem = cacheResult.Value;
                var mapped = Map(newsItem);
                return mapped;
            }
            else
            {
                return null;
            }
        }

        #endregion




        #region Map functions

        SavedNewsItem Map(Redis.DTOs.NewsItem o)
        {
            return new SavedNewsItem
            {
                SourceName = "temp",
                Title = o.Title,
                Link = o.Link,
                UtcPublishDateTime = o.UtcPublishDateTimeString,
                ImageUrl = o.ImageUrl,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Notes = null,
                Tags = null,
                Image = o.Image == null ? null : Map(o.Image),
            };
        }

        Image Map(Redis.DTOs.Image image)
        {
            return null;
        }

        #endregion




        #region Calls to the inner article service client

        async Task MarkRead(Guid userId, Guid articleId)
        {
            var article = await FindNewsItem(userId, articleId);
            if (article == null)
                return;

            await articleService.MarkRead(userId, article);
        }

        Task MarkUnread(Guid userId, Guid articleId)
        {
            return articleService.RemoveRead(userId, articleId);
        }

        async Task Favorite(Guid userId, Guid articleId)
        {
            var article = await FindNewsItem(userId, articleId);
            if (article == null)
                return;

            await articleService.AddFavorite(userId, article);
        }

        Task Unfavorite(Guid userId, Guid articleId)
        {
            return articleService.RemoveFavorite(userId, articleId);
        }

        #endregion
    }
}
