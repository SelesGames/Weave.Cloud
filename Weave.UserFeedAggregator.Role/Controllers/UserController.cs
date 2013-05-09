using Common.Caching;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.UserFeedAggregator.BusinessObjects;
using Weave.UserFeedAggregator.Contracts;
using Weave.UserFeedAggregator.DTOs;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;

namespace Weave.UserFeedAggregator.Role.Controllers
{
    public class UserController : ApiController, IWeaveUserService
    {
        IBasicCache<Guid, Task<UserInfo>> userCache;
        IUserWriter writer;

        public UserController(IBasicCache<Guid, Task<UserInfo>> userCache, IUserWriter writer)
        {
            this.userCache = userCache;
            this.writer = writer;
        }




        #region User creation

        [HttpPost]
        [ActionName("create")]
        public async Task<Outgoing.UserInfo> AddUserAndReturnUserInfo([FromBody] Incoming.UserInfo incomingUser)
        {
            var userBO = ConvertToBusinessObject(incomingUser);
            await writer.ImmediateWrite(userBO);
            await userBO.RefreshAllFeeds();
            writer.DelayedWrite(userBO);
            var outgoing = ConvertToOutgoing(userBO);
            return outgoing;
        }

        #endregion




        #region Get Basic User Info (suitable for panorama home screen)

        [HttpGet]
        [ActionName("info")]
        public async Task<Outgoing.UserInfo> GetUserInfo(Guid userId, bool refresh = false)
        {
            var userBO = await userCache.Get(userId);

            userBO.PreviousLoginTime = userBO.CurrentLoginTime;
            userBO.CurrentLoginTime = DateTime.UtcNow;

            if (refresh)
            {
                await userBO.RefreshAllFeeds();
                writer.DelayedWrite(userBO);
            }

            var outgoing = ConvertToOutgoing(userBO);
            outgoing.LatestNews = userBO.GetLatestArticles().Select(ConvertToOutgoing).ToList();
            return outgoing;
        }

        #endregion




        #region Get News for User (either by category or feedId)

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, string category, bool refresh = false, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            TimeSpan readTime, writeTime = TimeSpan.Zero;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var userBO = await userCache.Get(userId);
            sw.Stop();
            readTime = sw.Elapsed;

            var subset = userBO.CreateSubsetFromCategory(category);

            if (refresh)
            {
                await subset.Refresh();

                sw = System.Diagnostics.Stopwatch.StartNew();
                await writer.ImmediateWrite(userBO);
                sw.Stop();
                writeTime = sw.Elapsed;
            }

            var list = CreateNewsListFromSubset(userId, skip, take, type, requireImage, subset);
            list.DataStoreReadTime = readTime;
            list.DataStoreWriteTime = writeTime;
            return list;
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, Guid feedId, bool refresh = false, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            TimeSpan readTime, writeTime = TimeSpan.Zero;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var userBO = await userCache.Get(userId);
            sw.Stop();
            readTime = sw.Elapsed;

            var subset = userBO.CreateSubsetFromFeedIds(new[] { feedId });

            if (refresh)
            {
                await subset.Refresh();

                sw = System.Diagnostics.Stopwatch.StartNew();
                await writer.ImmediateWrite(userBO);
                sw.Stop();
                writeTime = sw.Elapsed;
            }

            var list = CreateNewsListFromSubset(userId, skip, take, type, requireImage, subset);
            list.DataStoreReadTime = readTime;
            list.DataStoreWriteTime = writeTime;
            return list;
        }

        Outgoing.NewsList CreateNewsListFromSubset(Guid userId, int skip, int take, NewsItemType type, bool requireImage, FeedsSubset subset)
        {
            var orderedNews = subset.AllOrderedNews().ToList();
            var totalNewsCount = orderedNews.Count;

            IEnumerable<NewsItem> outgoingNews = orderedNews;

            if (type == NewsItemType.New)
                outgoingNews = outgoingNews.Where(o => o.IsNew()).ToList();

            else if (type == NewsItemType.Viewed)
                outgoingNews = outgoingNews.Where(o => o.HasBeenViewed);

            else if (type == NewsItemType.Unviewed)
                outgoingNews = outgoingNews.Where(o => !o.HasBeenViewed);

            if (requireImage)
                outgoingNews = outgoingNews.Where(o => o.HasImage);

            outgoingNews = outgoingNews.Skip(skip).Take(take);

            var outgoing = new Outgoing.NewsList
            {
                UserId = userId,
                FeedCount = subset.Count(),
                TotalNewsCount = totalNewsCount,
                Take = take,
                Skip = skip,
                Feeds = subset.Select(ConvertToOutgoing).ToList(),
                News = outgoingNews.Select(ConvertToOutgoing).ToList(),
            };
            outgoing.NewsCount = outgoing.News.Count;
            outgoing.NewNewsCount = outgoing.Feeds.Sum(o => o.NewArticleCount);

            return outgoing;
        }

        #endregion




        #region Feed management

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId)
        {
            var userBO = await userCache.Get(userId);
            var output = new Outgoing.FeedsInfoList
            {
                UserId = userBO.Id,
                FeedCount = userBO.Feeds.Count,
                Feeds = userBO.Feeds.Select(ConvertToOutgoing).ToList(),
            };
            return output;
        }

        [HttpPost]
        [ActionName("add_feed")]
        public async Task AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            var userBO = await userCache.Get(userId);
            var feedBO = ConvertToBusinessObject(feed);
            userBO.AddFeed(feedBO);
            writer.DelayedWrite(userBO);
        }

        [HttpGet]
        [ActionName("remove_feed")]
        public async Task RemoveFeed(Guid userId, Guid feedId)
        {
            var userBO = await userCache.Get(userId);
            userBO.RemoveFeed(feedId);
            writer.DelayedWrite(userBO);
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            var userBO = await userCache.Get(userId);
            var feedBO = ConvertToBusinessObject(feed);
            userBO.UpdateFeed(feedBO);
            writer.DelayedWrite(userBO);
        }

        [HttpPost]
        [ActionName("batch_change")]
        public async Task BatchChange(Guid userId, [FromBody] Incoming.BatchFeedChange changeSet)
        {
            if (changeSet == null)
                return;

            var userBO = await userCache.Get(userId);

            var added = changeSet.Added;
            var removed = changeSet.Removed;
            var updated = changeSet.Updated;

            if (!EnumerableEx.IsNullOrEmpty(added))
            {
                foreach (var feed in added)
                {
                    var feedBO = ConvertToBusinessObject(feed);
                    userBO.AddFeed(feedBO);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(removed))
            {
                foreach (var feedId in removed)
                {
                    userBO.RemoveFeed(feedId);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(updated))
            {
                foreach (var feed in updated)
                {
                    var feedBO = ConvertToBusinessObject(feed);
                    userBO.UpdateFeed(feedBO);
                }
            }

            writer.DelayedWrite(userBO);
        }

        #endregion




        #region Article management

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid feedId, Guid newsItemId)
        {
            var userBO = await userCache.Get(userId);
            await userBO.MarkNewsItemRead(feedId, newsItemId);
            writer.DelayedWrite(userBO);
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid feedId, Guid newsItemId)
        {
            var userBO = await userCache.Get(userId);
            await userBO.MarkNewsItemUnread(feedId, newsItemId);
            writer.DelayedWrite(userBO);
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            var userBO = await userCache.Get(userId);
            userBO.MarkNewsItemsSoftRead(newsItemIds);
            writer.DelayedWrite(userBO);
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid feedId, Guid newsItemId)
        {
            var userBO = await userCache.Get(userId);
            await userBO.AddFavorite(feedId, newsItemId);
            writer.DelayedWrite(userBO);
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid feedId, Guid newsItemId)
        {
            var userBO = await userCache.Get(userId);
            await userBO.RemoveFavorite(feedId, newsItemId);
            writer.DelayedWrite(userBO);
        }

        #endregion




        #region Conversion Helpers

        UserInfo ConvertToBusinessObject(Incoming.UserInfo user)
        {
            return user.Convert<Incoming.UserInfo, UserInfo>(Converters.Converters.Instance);
        }

        Feed ConvertToBusinessObject(Incoming.NewFeed user)
        {
            return user.Convert<Incoming.NewFeed, Feed>(Converters.Converters.Instance);
        }

        Feed ConvertToBusinessObject(Incoming.UpdatedFeed user)
        {
            return user.Convert<Incoming.UpdatedFeed, Feed>(Converters.Converters.Instance);
        }
        
    
        Outgoing.NewsItem ConvertToOutgoing(NewsItem user)
        {
            return user.Convert<NewsItem, Outgoing.NewsItem>(Converters.Converters.Instance);
        }

        Outgoing.Feed ConvertToOutgoing(Feed user)
        {
            return user.Convert<Feed, Outgoing.Feed>(Converters.Converters.Instance);
        }

        Outgoing.UserInfo ConvertToOutgoing(UserInfo user)
        {
            return user.Convert<UserInfo, Outgoing.UserInfo>(Converters.Converters.Instance);
        }

        #endregion
    }
}