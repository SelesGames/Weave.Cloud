using Microsoft.WindowsAzure.StorageClient;
using SelesGames.Common;
using SelesGames.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.Article.Service.Contracts;
using Weave.User.BusinessObjects;
using Weave.User.Service.Cache;
using Weave.User.Service.Contracts;
using Weave.User.Service.Converters;
using Weave.User.Service.DTOs;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;

namespace Weave.User.Service.Role.Controllers
{
    public class UserController : ApiController, IWeaveUserService
    {
        UserRepository userRepo;
        IWeaveArticleService articleServiceClient;

        UserInfo userBO;
        TimeSpan readTime = TimeSpan.Zero, writeTime = TimeSpan.Zero;


        public UserController(UserRepository cacheClient, IWeaveArticleService articleServiceClient)
        {
            this.userRepo = cacheClient;
            this.articleServiceClient = articleServiceClient;
        }




        #region User creation

        [HttpGet]
        [ActionName("create")]
        public Task<Outgoing.UserInfo> Create()
        {
            return AddUserAndReturnUserInfo(null);
        }

        [HttpPost]
        [ActionName("create")]
        public async Task<Outgoing.UserInfo> AddUserAndReturnUserInfo([FromBody] Incoming.UserInfo incomingUser)
        {
            bool isIdEmpty = false;
            bool doesUserAlreadyExist = false;

            incomingUser = incomingUser ?? new Incoming.UserInfo();

            if (Guid.Empty == incomingUser.Id)
            {
                isIdEmpty = true;
                incomingUser.Id = Guid.NewGuid();
            }

            if (!isIdEmpty)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    userBO = await userRepo.Get(incomingUser.Id);
                    if (userBO != null)
                        doesUserAlreadyExist = true;
                }
                catch (StorageClientException)
                {
                    // if we get here, we couldn't find the user
                }
                catch
                {
                    throw;
                }
                finally
                {
                    sw.Stop();
                    readTime = sw.Elapsed;
                }
            }

            if (doesUserAlreadyExist)
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "A user with that Id already exists");
            }

            userBO = ConvertToBusinessObject(incomingUser);
            await userBO.RefreshAllFeeds();
            userRepo.Save(userBO.Id, userBO);
            var outgoing = ConvertToOutgoing(userBO);

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;

            return outgoing;
        }

        #endregion




        #region Get Basic User Info (suitable for panorama home screen)

        [HttpGet]
        [ActionName("info")]
        public async Task<Outgoing.UserInfo> GetUserInfo(Guid userId, bool refresh = false)
        {
            await VerifyUserId(userId);

            userBO.PreviousLoginTime = userBO.CurrentLoginTime;
            userBO.CurrentLoginTime = DateTime.UtcNow;

            userBO.DeleteOldNews();

            if (refresh)
            {
                await userBO.RefreshAllFeeds();
                SaveUser();
            }

            var outgoing = ConvertToOutgoing(userBO);
            outgoing.LatestNews = userBO.GetLatestArticles().Select(ConvertToOutgoing).ToList();

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;

            return outgoing;
        }

        #endregion




        #region Get News for User (either by category or feedId)

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, 
            string category = "all news", 
            EntryType entry = EntryType.Peek, 
            int skip = 0, 
            int take = 10, 
            NewsItemType type = NewsItemType.Any, 
            bool requireImage = false)
        {
            await VerifyUserId(userId);

            var subset = userBO.CreateSubsetFromCategory(category);

            if (entry == EntryType.Mark || entry == EntryType.ExtendRefresh)
            {
                if (entry == EntryType.Mark)
                {
                    userBO.DeleteOldNews();
                    subset.MarkEntry();
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    subset.ExtendEntry();
                    await subset.Refresh();
                }

                SaveUser();
            }

            var list = CreateNewsListFromSubset(userId, entry, skip, take, type, requireImage, subset);
            return list;
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, Guid feedId, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            await VerifyUserId(userId);

            var subset = userBO.CreateSubsetFromFeedIds(new[] { feedId });

            if (entry == EntryType.Mark || entry == EntryType.ExtendRefresh)
            {
                if (entry == EntryType.Mark)
                {
                    userBO.DeleteOldNews();
                    subset.MarkEntry();
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    subset.ExtendEntry();
                    await subset.Refresh();
                }

                SaveUser();
            }

            var list = CreateNewsListFromSubset(userId, entry, skip, take, type, requireImage, subset);
            return list;
        }

        #endregion




        #region Feed management

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);

            if (refresh)
            {
                await userBO.RefreshAllFeeds();
                SaveUser();
            }
            return CreateOutgoingFeedsInfoList(userBO.Feeds, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, string category, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);

            var subset = userBO.CreateSubsetFromCategory(category);
            if (refresh)
            {
                await subset.Refresh();
                SaveUser();
            }
            return CreateOutgoingFeedsInfoList(subset, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, Guid feedId, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);

            var subset = userBO.CreateSubsetFromFeedIds(new[] { feedId });
            if (refresh)
            {
                await subset.Refresh();
                SaveUser();
            }
            return CreateOutgoingFeedsInfoList(subset, nested);
        }

        [HttpPost]
        [ActionName("add_feed")]
        public async Task<Outgoing.Feed> AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            await VerifyUserId(userId);

            var feedBO = ConvertToBusinessObject(feed);
            if (userBO.AddFeed(feedBO))
            {
                SaveUser();
                return ConvertToOutgoing(feedBO);
            }
            else
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Feed not added");
            }
        }

        [HttpGet]
        [ActionName("remove_feed")]
        public async Task RemoveFeed(Guid userId, Guid feedId)
        {
            await VerifyUserId(userId);

            userBO.RemoveFeed(feedId);
            SaveUser();
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            await VerifyUserId(userId);

            var feedBO = ConvertToBusinessObject(feed);
            userBO.UpdateFeed(feedBO);
            SaveUser();
        }

        [HttpPost]
        [ActionName("batch_change")]
        public async Task BatchChange(Guid userId, [FromBody] Incoming.BatchFeedChange changeSet)
        {
            if (changeSet == null)
                return;

            await VerifyUserId(userId);

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

            SaveUser();
        }

        #endregion




        #region Article management

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            var newsItem = userBO.FindNewsItem(newsItemId);
            var saved = ConvertToArticleService(newsItem);
            await articleServiceClient.MarkRead(userId, saved);

            userBO.MarkNewsItemRead(newsItem);
            SaveUser();
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);
            await articleServiceClient.RemoveRead(userId, newsItemId);

            userBO.MarkNewsItemUnread(newsItemId);
            SaveUser();
        }

        [HttpPost]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            await VerifyUserId(userId);
            userBO.MarkNewsItemsSoftRead(newsItemIds);
            SaveUser();
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            await VerifyUserId(userId);
            userBO.MarkCategorySoftRead(category);
            SaveUser();
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            await VerifyUserId(userId);
            userBO.MarkFeedSoftRead(feedId);
            SaveUser();
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            var newsItem = userBO.FindNewsItem(newsItemId);
            var favorited = ConvertToArticleService(newsItem);
            await articleServiceClient.AddFavorite(userId, favorited);

            userBO.AddFavorite(newsItem);
            SaveUser();
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);
            await articleServiceClient.RemoveFavorite(userId, newsItemId);

            userBO.RemoveFavorite(newsItemId);
            SaveUser();
        }

        #endregion




        #region Article Expiry times (Marked Read and Unread Deletion times)

        public async Task SetArticleDeleteTimes(Guid userId, Incoming.ArticleDeleteTimes articleDeleteTimes)
        {
            await VerifyUserId(userId);

            userBO.ArticleDeletionTimeForMarkedRead = articleDeleteTimes.ArticleDeletionTimeForMarkedRead;
            userBO.ArticleDeletionTimeForUnread = articleDeleteTimes.ArticleDeletionTimeForUnread;
            SaveUser();
        }

        #endregion




        #region Helper methods

        async Task VerifyUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                    "You must specify a valid userId as a GUID");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                userBO = await userRepo.Get(userId);
            }
            catch (StorageClientException)
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound,
                    "No user found matching that userId");
            }
            catch
            {
                throw;
            }
            finally
            {
                sw.Stop();
                readTime = sw.Elapsed;
            }
        }

        Outgoing.NewsList CreateNewsListFromSubset(Guid userId, EntryType entry, int skip, int take, NewsItemType type, bool requireImage, FeedsSubset subset)
        {
            var orderedNews = subset.AllOrderedNews().ToList();

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
                Feeds = subset.Select(ConvertToOutgoing).ToList(),
                News = outgoingNews.Select(ConvertToOutgoing).ToList(),
                EntryType = entry.ToString(),
            };
            var page = new Outgoing.PageInfo
            {
                Skip = skip,
                Take = take,
                IncludedArticleCount = outgoing.News == null ? 0 : outgoing.News.Count,
            };
            outgoing.Page = page;

            outgoing.NewArticleCount = outgoing.Feeds == null ? 0 : outgoing.Feeds.Sum(o => o.NewArticleCount);
            outgoing.UnreadArticleCount = outgoing.Feeds == null ? 0 : outgoing.Feeds.Sum(o => o.UnreadArticleCount);
            outgoing.TotalArticleCount = outgoing.Feeds == null ? 0 : outgoing.Feeds.Sum(o => o.TotalArticleCount);

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;

            return outgoing;
        }

        void SaveUser()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                userRepo.Save(userBO.Id, userBO);
            }
            catch
            {
                throw;
            }
            finally
            {
                sw.Stop();
                writeTime = sw.Elapsed;
            }
        }

        #endregion




        #region Conversion Helpers

        UserInfo ConvertToBusinessObject(Incoming.UserInfo user)
        {
            return user.Convert<Incoming.UserInfo, UserInfo>(ServerIncomingToBusinessObject.Instance);
        }

        Feed ConvertToBusinessObject(Incoming.NewFeed user)
        {
            return user.Convert<Incoming.NewFeed, Feed>(ServerIncomingToBusinessObject.Instance);
        }

        Feed ConvertToBusinessObject(Incoming.UpdatedFeed user)
        {
            return user.Convert<Incoming.UpdatedFeed, Feed>(ServerIncomingToBusinessObject.Instance);
        }
        
    
        Outgoing.NewsItem ConvertToOutgoing(NewsItem user)
        {
            return user.Convert<NewsItem, Outgoing.NewsItem>(BusinessObjectToServerOutgoing.Instance);
        }

        Outgoing.Feed ConvertToOutgoing(Feed user)
        {
            return user.Convert<Feed, Outgoing.Feed>(BusinessObjectToServerOutgoing.Instance);
        }

        Outgoing.UserInfo ConvertToOutgoing(UserInfo user)
        {
            return user.Convert<UserInfo, Outgoing.UserInfo>(BusinessObjectToServerOutgoing.Instance);
        }


        Outgoing.FeedsInfoList CreateOutgoingFeedsInfoList(IEnumerable<Feed> feeds, bool returnNested)
        {
            Outgoing.FeedsInfoList outgoing = new Outgoing.FeedsInfoList
            {
                UserId = userBO.Id,
                TotalFeedCount = userBO.Feeds == null ? 0 : userBO.Feeds.Count,
            };

            if (EnumerableEx.IsNullOrEmpty(feeds))
            {
                outgoing.NewArticleCount = 0;
                outgoing.UnreadArticleCount = 0;
                outgoing.TotalArticleCount = 0;
                return outgoing;
            }

            var outgoingFeeds = feeds.Select(ConvertToOutgoing).ToList();
            outgoing.NewArticleCount = outgoingFeeds.Sum(o => o.NewArticleCount);
            outgoing.UnreadArticleCount = outgoingFeeds.Sum(o => o.UnreadArticleCount);
            outgoing.TotalArticleCount = outgoingFeeds.Sum(o => o.TotalArticleCount);

            if (returnNested)
            {
                var grouped = outgoingFeeds.GroupBy(o => o.Category);
                var categories = grouped.Where(o => !string.IsNullOrWhiteSpace(o.Key))
                    .Select(o => CreateCategory(o.Key, o == null ? null : o.ToList()))
                    .OrderBy(o => o.Category)
                    .ToList();

                outgoing.Categories = categories;
                outgoing.Feeds = grouped.Where(o => string.IsNullOrWhiteSpace(o.Key)).SelectMany(o => o).ToList();
            }
            else
            {
                outgoing.Feeds = outgoingFeeds;
            }

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;

            return outgoing;
        }

        Outgoing.CategoryInfo CreateCategory(string categoryName, List<Outgoing.Feed> feeds)
        {
            return new Outgoing.CategoryInfo
            {
                Category = categoryName,
                TotalFeedCount = feeds == null ? 0 : feeds.Count,
                Feeds = feeds,
                NewArticleCount = feeds == null ? 0 : feeds.Sum(o => o.NewArticleCount),
                UnreadArticleCount = feeds == null ? 0 : feeds.Sum(o => o.UnreadArticleCount),
                TotalArticleCount = feeds == null ? 0 : feeds.Sum(o => o.TotalArticleCount),
            };
        }

        Weave.Article.Service.DTOs.ServerIncoming.SavedNewsItem ConvertToArticleService(NewsItem newsItem)
        {
            return newsItem.Convert<NewsItem, Weave.Article.Service.DTOs.ServerIncoming.SavedNewsItem>(Converters.Converters.Instance);
        }

        #endregion
    }
}