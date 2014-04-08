﻿using Microsoft.WindowsAzure.Storage;
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
using Weave.User.BusinessObjects.v2;
//using Weave.User.Service.Cache;
using Weave.User.Service.Contracts;
using Weave.User.Service.Converters.v2;
using Weave.User.Service.DTOs;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;

namespace Weave.User.Service.WorkerRole.v2.Controllers
{
    public class UserController : ApiController, IWeaveUserService
    {
        Guid userId;
        TimeSpan readTime = TimeSpan.Zero, writeTime = TimeSpan.Zero;
        readonly IWeaveArticleService articleServiceClient;

        public UserController(IWeaveArticleService articleServiceClient)
        {
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
                catch (StorageException)
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

            if (refresh)
            {
                await userBO.RefreshAllFeeds();
            }

            userBO.DeleteOldNews();
            SaveUser();

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
                    subset.MarkEntry();
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    subset.ExtendEntry();
                    await subset.Refresh();
                }

                userBO.DeleteOldNews();
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
                    subset.MarkEntry();
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    subset.ExtendEntry();
                    await subset.Refresh();
                }

                userBO.DeleteOldNews();
                SaveUser();
            }

            var list = CreateNewsListFromSubset(userId, entry, skip, take, type, requireImage, subset);
            return list;
        }

        #endregion




        #region Get Feeds for User

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);

            var user = await GetUser();
            var allnews = await GetAllNews();
            var cache = await GetNewsItemStateCache();

            if (refresh)
            {
                var subset = new FeedsSubset(user.Feeds);
                await subset.Refresh(allnews);

                Update(cache, allnews);

                await Save(cache);
                await Save(allnews);
                await Save(user);
            }

            return CreateOutgoingFeedsInfoList(user, allnews, cache, nested);
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

        #endregion




        #region Feed management  FULLY STUBBED

        [HttpPost]
        [ActionName("add_feed")]
        public async Task<Outgoing.Feed> AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            await VerifyUserId(userId);

            var user = await GetUser();
            var feedBO = ConvertToBusinessObject(feed);

            if (user.Feeds.TryAdd(feedBO))
            {
                await Save(user);
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

            var user = await GetUser();
            user.Feeds.RemoveWithId(feedId);
            await Save(user);
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            await VerifyUserId(userId);

            var user = await GetUser();
            var feedBO = ConvertToBusinessObject(feed);

            user.Feeds.Update(feedBO);
            await Save(user);
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

            var user = await GetUser();

            if (!EnumerableEx.IsNullOrEmpty(added))
            {
                foreach (var feed in added)
                {
                    var feedBO = ConvertToBusinessObject(feed);
                    user.Feeds.TryAdd(feedBO);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(removed))
            {
                foreach (var feedId in removed)
                {
                    user.Feeds.RemoveWithId(feedId);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(updated))
            {
                foreach (var feed in updated)
                {
                    var feedBO = ConvertToBusinessObject(feed);
                    user.Feeds.Update(feedBO);
                }
            }

            await Save(user);
        }

        #endregion




        #region Article management  FULLY STUBBED

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateMediator(newsItemStateCache);
            mediator.MarkNewsItemRead(newsItemId);
            await Save(newsItemStateCache);
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateMediator(newsItemStateCache);
            mediator.MarkNewsItemUnread(newsItemId);
            await Save(newsItemStateCache);
        }

        [HttpPost]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            await VerifyUserId(userId);

            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateMediator(newsItemStateCache);
            mediator.MarkNewsItemsSoftRead(newsItemIds);
            await Save(newsItemStateCache);
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            await VerifyUserId(userId);

            var user = await GetUser();
            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateExtendedMediator(user, newsItemStateCache);
            mediator.MarkCategorySoftRead(category);
            await Save(newsItemStateCache);
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            await VerifyUserId(userId);

            var user = await GetUser();
            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateExtendedMediator(user, newsItemStateCache);
            mediator.MarkFeedSoftRead(feedId);
            await Save(newsItemStateCache);
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateMediator(newsItemStateCache);
            mediator.AddFavorite(newsItemId);
            await Save(newsItemStateCache);
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            var newsItemStateCache = await GetNewsItemStateCache();

            var mediator = new UserArticleStateMediator(newsItemStateCache);
            mediator.RemoveFavorite(newsItemId);
            await Save(newsItemStateCache);
        }

        #endregion




        #region Article Expiry times (Marked Read and Unread Deletion times)  FULLY STUBBED

        [HttpPost]
        [ActionName("set_delete_times")]
        public async Task SetArticleDeleteTimes(Guid userId, Incoming.ArticleDeleteTimes articleDeleteTimes)
        {
            await VerifyUserId(userId);

            var user = await GetUser();

            user.ArticleDeletionTimeForMarkedRead = articleDeleteTimes.ArticleDeletionTimeForMarkedRead;
            user.ArticleDeletionTimeForUnread = articleDeleteTimes.ArticleDeletionTimeForUnread;
 
            await Save(user);
        }



        #endregion




        #region Helper methods

        async Task VerifyUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                    "You must specify a valid userId as a GUID");

            this.userId = userId;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                //userBO = await userRepo.Get(userId);
            }
            catch (StorageException)
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

        Outgoing.NewsList CreateNewsListFromSubset(
            EntryType entry, 
            int skip, 
            int take, 
            NewsItemType type, 
            bool requireImage, 
            IEnumerable<Feed> subset,
            MasterNewsItemCollection allNews,
            NewsItemStateCache stateCache)
        {
            var feedGen = new ExtendedFeedsMediator(allNews, stateCache);
            var extendedFeeds = feedGen.GetExtendedInfo(subset);
            var orderedNews = extendedFeeds.AllOrderedNews();

            IEnumerable<ExtendedNewsItem> outgoingNews = orderedNews;

            if (type == NewsItemType.New)
                outgoingNews = outgoingNews.Where(o => o.IsNew()).ToList();

            else if (type == NewsItemType.Viewed)
                outgoingNews = outgoingNews.Where(o => o.HasBeenViewed);

            else if (type == NewsItemType.Unviewed)
                outgoingNews = outgoingNews.Where(o => !o.HasBeenViewed);

            if (requireImage)
                outgoingNews = outgoingNews.Where(o => o.NewsItem.HasImage);

            outgoingNews = outgoingNews.Skip(skip).Take(take);

            var outgoing = new Outgoing.NewsList
            {
                UserId = userId,
                FeedCount = extendedFeeds.Count(),
                Feeds = extendedFeeds.Select(ConvertToOutgoing).ToList(),
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

        //void SaveUser()
        //{
        //    var sw = System.Diagnostics.Stopwatch.StartNew();

        //    try
        //    {
        //        userRepo.Save(userBO.Id, userBO);
        //    }
        //    catch(Exception e)
        //    {
        //        DebugEx.WriteLine(e);
        //        throw;
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        writeTime = sw.Elapsed;
        //    }
        //}

        #endregion




        #region Conversion Helpers




        #region from incoming to business object

        UserInfo ConvertToBusinessObject(Incoming.UserInfo user)
        {
            return ServerIncomingToBusinessObject.Instance.Convert(user);
        }

        Feed ConvertToBusinessObject(Incoming.NewFeed newFeed)
        {
            return ServerIncomingToBusinessObject.Instance.Convert(newFeed);
        }

        Feed ConvertToBusinessObject(Incoming.UpdatedFeed updatedFeed)
        {
            return ServerIncomingToBusinessObject.Instance.Convert(updatedFeed);
        }

        #endregion




        #region from business object to outgoing

        Outgoing.NewsItem ConvertToOutgoing(ExtendedNewsItem newsItem)
        {
            return BusinessObjectToServerOutgoing.Instance.Convert(newsItem);
        }

        Outgoing.Feed ConvertToOutgoing(Feed feed)
        {
            return BusinessObjectToServerOutgoing.Instance.Convert(feed);
        }

        Outgoing.Feed ConvertToOutgoing(ExtendedFeed feed)
        {
            return BusinessObjectToServerOutgoing.Instance.Convert(feed);
        }

        Outgoing.UserInfo ConvertToOutgoing(UserInfo user)
        {
            return BusinessObjectToServerOutgoing.Instance.Convert(user);
        }

        #endregion




        Outgoing.FeedsInfoList CreateOutgoingFeedsInfoList(
            UserInfo user,
            MasterNewsItemCollection allNews,
            NewsItemStateCache stateCache,
            bool returnNested)
        {
            var feeds = user.Feeds;

            Outgoing.FeedsInfoList outgoing = new Outgoing.FeedsInfoList
            {
                UserId = user.Id,
                TotalFeedCount = feeds == null ? 0 : feeds.Count,
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




        Task Save(UserInfo user)
        {
            throw new NotImplementedException();
        }

        Task Save(NewsItemStateCache newsItemStateCache)
        {
            throw new NotImplementedException();
        }

        Task Save(MasterNewsItemCollection allnews)
        {
            throw new NotImplementedException();
        }

        Task<UserInfo> GetUser()
        {
            throw new NotImplementedException();
        }

        Task<MasterNewsItemCollection> GetAllNews()
        {
            throw new NotImplementedException();
        }

        Task<NewsItemStateCache> GetNewsItemStateCache()
        {
            throw new NotImplementedException();
        }

        void Update(NewsItemStateCache cache, MasterNewsItemCollection allnews)
        {
            throw new NotImplementedException();
        }
    }
}