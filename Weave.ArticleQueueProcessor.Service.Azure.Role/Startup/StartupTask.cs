using Common.Compression;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Article.Service.DTOs.ServerIncoming;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.InterRoleMessaging.Articles;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Clients;

namespace Weave.ArticleQueueProcessor.Service.Azure.Role.Startup
{
    internal class StartupTask
    {
        readonly TimeSpan pollingInterval = TimeSpan.FromMilliseconds(30);
        readonly Article.Service.Client.ServiceClient articleService;
        readonly ArticleStateChangeMessageQueue messageQueue;
        readonly Weave.FeedUpdater.BusinessObjects.Cache.ExpandedEntryCache newsCache;

        public StartupTask()
        {
            Settings.CompressionHandlers = new Common.Compression.Windows.CompressionHandlerCollection();
            this.articleService = new Article.Service.Client.ServiceClient();

            var connectionMultiplexer = Weave.Services.Redis.Ambient.Settings.StandardConnection;
            this.messageQueue = new ArticleStateChangeMessageQueue(connectionMultiplexer);

            // create the cache with a local cache size of 0
            this.newsCache = Weave.FeedUpdater.BusinessObjects.Cache.ExpandedEntryCacheFactory.CreateCache(0);
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
            var source = message.Value.Source;

            if (change == ArticleStateChange.Read)
                await MarkRead(userId, articleId, source);

            else if (change == ArticleStateChange.Unread)
                await MarkUnread(userId, articleId);

            else if (change == ArticleStateChange.Favorite)
                await Favorite(userId, articleId, source);

            else if (change == ArticleStateChange.Unfavorite)
                await Unfavorite(userId, articleId);

            var isMessageRemoved = await message.Complete();

            if (isMessageRemoved)
                System.Diagnostics.Debug.WriteLine(string.Format(
                "{0} removed from process queue", articleId));
        }




        #region Find the News Item from the Redis cache (or the Azure Blob cache)

        async Task<SavedNewsItem> FindNewsItem(Guid userId, Guid articleId)
        {
            var cacheResults = await newsCache.Get(new[] { articleId });
            var cacheResult = cacheResults.Results.FirstOrDefault();

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

        SavedNewsItem Map(ExpandedEntry o)
        {
            var bestImage = o.Images.GetBest();

            return new SavedNewsItem
            {
                //SourceName = "temp",
                Title = o.Title,
                Link = o.Link,
                UtcPublishDateTime = o.UtcPublishDateTimeString,
                ImageUrl = bestImage == null ? null : bestImage.Url,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Notes = null,
                Tags = null,
                Image = bestImage == null ? null : Map(bestImage),
            };
        }

        Weave.Article.Service.DTOs.Image Map(Updater.BusinessObjects.Image o)
        {
            return new Weave.Article.Service.DTOs.Image
            {      
                Width = o.Width,
                Height = o.Height,
                BaseImageUrl = o.Url,
                OriginalUrl = o.Url,
                SupportedFormats = null,
            };
        }

        #endregion




        #region Calls to the inner article service client

        async Task MarkRead(Guid userId, Guid articleId, string source)
        {
            var article = await FindNewsItem(userId, articleId);
            if (article == null)
                return;

            article.SourceName = source;

            await articleService.MarkRead(userId, article);
        }

        Task MarkUnread(Guid userId, Guid articleId)
        {
            return articleService.RemoveRead(userId, articleId);
        }

        async Task Favorite(Guid userId, Guid articleId, string source)
        {
            var article = await FindNewsItem(userId, articleId);
            if (article == null)
                return;

            article.SourceName = source;

            await articleService.AddFavorite(userId, article);
        }

        Task Unfavorite(Guid userId, Guid articleId)
        {
            return articleService.RemoveFavorite(userId, articleId);
        }

        #endregion
    }
}