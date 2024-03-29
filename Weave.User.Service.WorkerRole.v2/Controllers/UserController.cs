﻿using Microsoft.WindowsAzure.Storage;
using SelesGames.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.Article.Service.Contracts;
using Weave.User.BusinessObjects.v2;
using Weave.User.BusinessObjects.v2.Repositories;
using Weave.User.Service.Contracts;
using Weave.User.Service.Converters.v2;
using Weave.User.Service.DTOs;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;

namespace Weave.User.Service.WorkerRole.v2.Controllers
{
    public class UserController : ApiController, IWeaveUserService
    {
        readonly IUserInfoRepository repo;
        readonly IWeaveArticleService articleServiceClient;

        Guid userId;
        TimeSpan readTime = TimeSpan.Zero, writeTime = TimeSpan.Zero;

        UserInfo user;
        MasterNewsItemCollection allNews;
        NewsItemStateCache stateCache;




        #region Private member Properties that will throw exception is accessed before being initialized (i.e. hydrated)

        new UserInfo User
        {
            get
            {
                if (user == null)
                    throw new Exception("you must hydrate user before using it");

                return user;
            }
        }

        MasterNewsItemCollection AllNews
        {
            get
            {
                if (allNews == null)
                    throw new Exception("you must hydrate allNews before using it");

                return allNews;
            }
        }

        NewsItemStateCache StateCache
        {
            get
            {
                if (stateCache == null)
                    throw new Exception("you must hydrate stateCache before using it");

                return stateCache;
            }
        }

        #endregion




        #region Constructor

        public UserController(IUserInfoRepository repo, IWeaveArticleService articleServiceClient)
        {
            this.repo = repo;
            this.articleServiceClient = articleServiceClient;
        }

        #endregion




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

            UserInfo user;

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
                    this.userId = incomingUser.Id;
                    user = await GetUser();
                    if (user != null)
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

            user = ConvertToBusinessObject(incomingUser);

            allNews = new MasterNewsItemCollection { UserId = user.Id };
            stateCache = new NewsItemStateCache { UserId = user.Id };

            var updater = new FeedsUpdateMediator(user.Feeds, allNews, stateCache);
            await updater.Refresh();

            CreateNewStateCache();

            await All(
                Save(user),
                Save(allNews),
                Save(stateCache)
            );


            var outgoing = ConvertToOutgoing(user);

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
            await All(
                HydrateUser(),
                HydrateAllNews(),
                HydrateStateCache()
            );

            User.PreviousLoginTime = User.CurrentLoginTime;
            User.CurrentLoginTime = DateTime.UtcNow;

            if (refresh)
            {


                var updater = new FeedsUpdateMediator(User.Feeds, AllNews, StateCache);
                await updater.Refresh();
            }

            var cleaner = new NewsCleanupMediator(User, AllNews, StateCache);
            cleaner.DeleteOldNews();
            await Save(User);

            var outgoing = ConvertToOutgoing(User);
            //outgoing.LatestNews = userBO.GetLatestArticles().Select(ConvertToOutgoing).ToList();

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;

            return outgoing;
        }

        #endregion




        #region Get News for User (either by category or feedId)  FULLY STUBBED

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
            await All(
                HydrateUser(),
                HydrateAllNews(),
                HydrateStateCache()
            );

            var subset = User.Feeds.FindByCategory(category);
            return await GetNews(subset, entry, skip, take, type, requireImage);
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, Guid feedId, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            await VerifyUserId(userId);
            await All(
                HydrateUser(),
                HydrateAllNews(),
                HydrateStateCache()
            );

            var subset = User.Feeds.FindById(feedId);
            return await GetNews(subset, entry, skip, take, type, requireImage);
        }

        async Task<Outgoing.NewsList> GetNews(
            IEnumerable<Feed> feeds, 
            EntryType entry = EntryType.Peek, 
            int skip = 0, 
            int take = 10, 
            NewsItemType type = NewsItemType.Any, 
            bool requireImage = false)
        {
            if (entry == EntryType.Mark || entry == EntryType.ExtendRefresh)
            {
                if (entry == EntryType.Mark)
                {
                    feeds.MarkEntry();

                    await All(Save(User));
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    feeds.ExtendEntry();
                    var updater = new FeedsUpdateMediator(feeds, AllNews, StateCache);
                    await updater.Refresh();

                    CreateNewStateCache();

                    await All(
                        Save(User),
                        Save(AllNews),
                        Save(StateCache)
                    );
                }
            }

            var list = CreateNewsListFromSubset(entry, skip, take, type, requireImage, feeds);
            return list;
        }

        #endregion




        #region Get Feeds for User  FULLY STUBBED

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);
            await All(
                HydrateUser(),
                HydrateAllNews(),
                HydrateStateCache()
            );

            var feeds = User.Feeds;
            return await GetFeeds(feeds, refresh, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, string category, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);
            await All(
                HydrateUser(),
                HydrateAllNews(),
                HydrateStateCache()
            );

            var subset = User.Feeds.FindByCategory(category);
            return await GetFeeds(subset, refresh, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, Guid feedId, bool refresh = false, bool nested = false)
        {
            await VerifyUserId(userId);
            await All(
                HydrateUser(),
                HydrateAllNews(),
                HydrateStateCache()
            );

            var subset = User.Feeds.FindById(feedId);
            return await GetFeeds(subset, refresh, nested);
        }

        async Task<Outgoing.FeedsInfoList> GetFeeds(IEnumerable<Feed> feeds, bool refresh, bool isNested)
        {
            if (refresh)
            {
                var updater = new FeedsUpdateMediator(feeds, AllNews, StateCache);
                await updater.Refresh();

                CreateNewStateCache();

                await All(
                    Save(User),
                    Save(AllNews),
                    Save(StateCache)
                );
            }

            return CreateOutgoingFeedsInfoList(feeds, isNested);
        }

        #endregion




        #region Feed management  FULLY STUBBED

        [HttpPost]
        [ActionName("add_feed")]
        public async Task<Outgoing.Feed> AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            await VerifyUserId(userId);
            await HydrateUser();

            var feedBO = ConvertToBusinessObject(feed);

            if (User.Feeds.TryAdd(feedBO))
            {
                await Save(User);
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
            await HydrateUser();

            User.Feeds.RemoveWithId(feedId);
            await Save(User);
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            await VerifyUserId(userId);
            await HydrateUser();

            var feedBO = ConvertToBusinessObject(feed);

            User.Feeds.Update(feedBO);
            await Save(User);
        }

        [HttpPost]
        [ActionName("batch_change")]
        public async Task BatchChange(Guid userId, [FromBody] Incoming.BatchFeedChange changeSet)
        {
            if (changeSet == null)
                return;

            await VerifyUserId(userId);
            await HydrateUser();

            var added = changeSet.Added;
            var removed = changeSet.Removed;
            var updated = changeSet.Updated;

            if (!EnumerableEx.IsNullOrEmpty(added))
            {
                foreach (var feed in added)
                {
                    var feedBO = ConvertToBusinessObject(feed);
                    User.Feeds.TryAdd(feedBO);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(removed))
            {
                foreach (var feedId in removed)
                {
                    User.Feeds.RemoveWithId(feedId);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(updated))
            {
                foreach (var feed in updated)
                {
                    var feedBO = ConvertToBusinessObject(feed);
                    User.Feeds.Update(feedBO);
                }
            }

            await Save(User);
        }

        #endregion




        #region Article management  FULLY STUBBED

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);
            await HydrateStateCache();

            var mediator = new UserArticleStateMediator(StateCache);
            mediator.MarkNewsItemRead(newsItemId);
            await Save(StateCache);
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);
            await HydrateStateCache();

            var mediator = new UserArticleStateMediator(StateCache);
            mediator.MarkNewsItemUnread(newsItemId);
            await Save(StateCache);
        }

        [HttpPost]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            await VerifyUserId(userId);
            await HydrateStateCache();

            var mediator = new UserArticleStateMediator(StateCache);
            mediator.MarkNewsItemsSoftRead(newsItemIds);
            await Save(StateCache);
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            await VerifyUserId(userId);
            await All(
                HydrateUser(),
                HydrateStateCache()
            );

            var mediator = new UserArticleStateExtendedMediator(User, StateCache);
            mediator.MarkCategorySoftRead(category);
            await Save(StateCache);
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            await VerifyUserId(userId);
            await All(
                HydrateUser(),
                HydrateStateCache()
            );

            var mediator = new UserArticleStateExtendedMediator(User, StateCache);
            mediator.MarkFeedSoftRead(feedId);
            await Save(StateCache);
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);
            await HydrateStateCache();

            var mediator = new UserArticleStateMediator(StateCache);
            mediator.AddFavorite(newsItemId);
            await Save(StateCache);
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);
            await HydrateStateCache();

            var mediator = new UserArticleStateMediator(StateCache);
            mediator.RemoveFavorite(newsItemId);
            await Save(StateCache);
        }

        #endregion




        #region Article Expiry times (Marked Read and Unread Deletion times)  FULLY STUBBED

        [HttpPost]
        [ActionName("set_delete_times")]
        public async Task SetArticleDeleteTimes(Guid userId, Incoming.ArticleDeleteTimes articleDeleteTimes)
        {
            await VerifyUserId(userId);
            await HydrateUser();

            User.ArticleDeletionTimeForMarkedRead = articleDeleteTimes.ArticleDeletionTimeForMarkedRead;
            User.ArticleDeletionTimeForUnread = articleDeleteTimes.ArticleDeletionTimeForUnread;
 
            await Save(User);
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
            IEnumerable<Feed> subset)
        {
            var feedGen = new ExtendedFeedsMediator(AllNews, StateCache);
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

        /// <summary>
        /// Encapsulates Task.Whenall just to make it a bit prettier
        /// </summary>
        async Task All(params Task[] tasks)
        {
            try
            {
                await Task.WhenAll(tasks);
                DebugEx.WriteLine(tasks);
            }
            catch(Exception ex)
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, ex.Message);
            }
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



        /// <summary>
        /// We can definitely rewrite this to not have to depend on AllNews.
        /// It would require storing the Category and FeedId for each NewsItem entry 
        /// in the NewsItemStateCache
        /// </summary>
        Outgoing.FeedsInfoList CreateOutgoingFeedsInfoList(IEnumerable<Feed> feeds, bool returnNested)
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
            {
                return new Outgoing.FeedsInfoList
                {
                    UserId = User.Id,
                    NewArticleCount = 0,
                    UnreadArticleCount = 0,
                    TotalArticleCount = 0,
                    TotalFeedCount = 0,
                };
            }

            var generator = new ExtendedFeedsMediator(AllNews, StateCache);
            var extendedFeeds = generator.GetExtendedInfo(feeds).ToList();

            Outgoing.FeedsInfoList outgoing = new Outgoing.FeedsInfoList
            {
                UserId = User.Id,
                TotalFeedCount = extendedFeeds.Count,
            };

            var outgoingFeeds = extendedFeeds.Select(ConvertToOutgoing).ToList();

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

        Weave.Article.Service.DTOs.ServerIncoming.SavedNewsItem ConvertToArticleService(ExtendedNewsItem newsItem)
        {
            return Converters.Converters.Instance.Convert(newsItem);
        }

        #endregion




        #region Recovery/hydration of necessary fields

        async Task HydrateUser()
        {
            if (user != null)
                return;

            user = await GetUser();
        }

        async Task HydrateAllNews()
        {
            if (allNews != null)
                return;

            allNews = await GetAllNews();
        }

        async Task HydrateStateCache()
        {
            if (stateCache != null)
                return;

            stateCache = await GetNewsItemStateCache();
        }

        Task<UserInfo> GetUser()
        {
            return repo.GetUser(userId);
        }

        Task<MasterNewsItemCollection> GetAllNews()
        {
            return repo.GetAllNews(userId);
        }

        Task<NewsItemStateCache> GetNewsItemStateCache()
        {
            return repo.GetNewsItemStateCache(userId);
        }

        #endregion




        #region Saves

        Task Save(UserInfo user)
        {
            return repo.Save(user);
        }

        Task Save(NewsItemStateCache newsItemStateCache)
        {
            return repo.Save(newsItemStateCache);
        }

        Task Save(MasterNewsItemCollection allnews)
        {
            return repo.Save(allnews);
        }

        #endregion




        void CreateNewStateCache()
        {
            var news = AllNews.Values.SelectMany(o => o).ToList();
            var matched = StateCache.MatchingIds(news.Select(o => o.Id));
            var newCache = new NewsItemStateCache { UserId = StateCache.UserId };

            foreach (var match in matched)
            {
                newCache.Add(match.Key, match);
            }
            
            foreach (var item in news)
            {
                if (!newCache.ContainsKey(item.Id))
                {
                    newCache.Add(item.Id, new NewsItemState());
                }
            }

            stateCache = newCache;
        }
    }
}