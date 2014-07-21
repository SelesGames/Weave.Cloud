using Microsoft.WindowsAzure.Storage;
using SelesGames.Common;
using SelesGames.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.User.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Cache;
using Weave.User.Service.Contracts;
using Weave.User.Service.Converters;
using Weave.User.Service.DTOs;
using Weave.User.Service.Redis;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;

namespace Weave.User.Service.Role.Controllers
{
    #region article queue service

    public class MockArticleQueueService : IArticleQueueService
    {
        public void QueueMarkRead(Guid userId, Guid newsItemId)
        {
        }

        public void QueueMarkUnread(Guid userId, Guid newsItemId)
        {
        }

        public void QueueAddFavorite(Guid userId, Guid newsItemId)
        {
        }

        public void QueueRemoveFavorite(Guid userId, Guid newsItemId)
        {
        }
    }

    public interface IArticleQueueService
    {
        void QueueMarkRead(Guid userId, Guid newsItemId);
        void QueueMarkUnread(Guid userId, Guid newsItemId);
        void QueueAddFavorite(Guid userId, Guid newsItemId);
        void QueueRemoveFavorite(Guid userId, Guid newsItemId);
    }

    #endregion




    public class UserController : ApiController, IWeaveUserService
    {
        #region Private member variables + constructor

        readonly UserRepository userRepo;
        readonly UserIndexCache userIndexCache;
        readonly NewsItemCache newsItemCache;
        readonly IArticleQueueService articleQueueService;

        Guid userId;
        UserInfo userBO;
        UserIndex userIndex;

        TimeSpan 
            readTime = TimeSpan.Zero, 
            writeTime = TimeSpan.Zero;

        public UserController(
            UserRepository userRepo,
            UserIndexCache userIndexCache,
            NewsItemCache newsItemCache,
            IArticleQueueService articleQueueService)
        {
            this.userRepo = userRepo;
            this.userIndexCache = userIndexCache;
            this.newsItemCache = newsItemCache;
            this.articleQueueService = articleQueueService;
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
            this.userId = userBO.Id;
            await PerformRefreshOnFeeds(userBO.Feeds);


            var outgoing = ConvertToOutgoing(userIndex);

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
            if (refresh)
            {
                await LoadBoth(userId);

                userBO.PreviousLoginTime = userBO.CurrentLoginTime;
                userBO.CurrentLoginTime = DateTime.UtcNow;

                var feeds = userBO.Feeds;
                await PerformRefreshOnFeeds(feeds);
            }
            else
            {
                await LoadIndexOnly(userId);

                userIndex.PreviousLoginTime = userIndex.CurrentLoginTime;
                userIndex.CurrentLoginTime = DateTime.UtcNow;
            }

            var outgoing = ConvertToOutgoing(userIndex);
            //outgoing.LatestNews = userBO.GetLatestArticles().Select(ConvertToOutgoing).ToList();

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;

            return outgoing;


            ///************************************
            //await LoadBoth(userId);

            //userBO.PreviousLoginTime = userBO.CurrentLoginTime;
            //userBO.CurrentLoginTime = DateTime.UtcNow;

            //if (refresh)
            //{
            //    await userBO.RefreshAllFeeds();
            //}

            //userBO.DeleteOldNews();

            //userIndex = userBO.CreateUserIndex();
            //await SaveUserIndex();

            //var outgoing = ConvertToOutgoing(userBO);
            //outgoing.LatestNews = userBO.GetLatestArticles().Select(ConvertToOutgoing).ToList();

            //outgoing.DataStoreReadTime = readTime;
            //outgoing.DataStoreWriteTime = writeTime;

            //return outgoing;
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
            IEnumerable<FeedIndex> feeds;
            category = category ?? "all news";

            if (entry == EntryType.ExtendRefresh)
            {
                await LoadBoth(userId);

                var feedsBO = userBO.Feeds.OfCategory(category);
                var subset = new FeedsSubset(feedsBO);
                subset.ExtendEntry();
                await PerformRefreshOnFeeds(feedsBO);
                feeds = userIndex.FeedIndices.OfCategory(category);
            }
            else if (entry == EntryType.Mark)
            {
                await LoadIndexOnly(userId, saveOnFail: false);
                feeds = userIndex.FeedIndices.OfCategory(category);
                feeds.MarkEntry();
                await SaveUserIndex();
            }
            else
            {
                await LoadIndexOnly(userId, saveOnFail: true);
                feeds = userIndex.FeedIndices.OfCategory(category);
            }

            var list = await CreateNewsListFromSubset(
                feeds: feeds,
                skip: skip, 
                take: take, 
                type: type,
                entry: entry,
                requireImage: requireImage);

            return list;
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, Guid feedId, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            IEnumerable<FeedIndex> feeds;

            if (entry == EntryType.ExtendRefresh)
            {
                await LoadBoth(userId);

                var feedsBO = userBO.Feeds.WithId(feedId).ToList();
                var subset = new FeedsSubset(feedsBO);
                subset.ExtendEntry();
                await PerformRefreshOnFeeds(feedsBO);
                feeds = userIndex.FeedIndices.WithId(feedId).ToList();
            }
            else if (entry == EntryType.Mark)
            {
                await LoadIndexOnly(userId, saveOnFail: false);
                feeds = userIndex.FeedIndices.WithId(feedId).ToList();
                feeds.MarkEntry();
                await SaveUserIndex();
            }
            else
            {
                await LoadIndexOnly(userId, saveOnFail: true);
                feeds = userIndex.FeedIndices.WithId(feedId).ToList();
            }

            var list = await CreateNewsListFromSubset(
                feeds: feeds,
                skip: skip,
                take: take,
                type: type,
                entry: entry,
                requireImage: requireImage);

            return list;
        }

        #endregion




        #region Feed information

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, bool refresh = false, bool nested = false)
        {
            if (refresh)
            {
                await LoadBoth(userId);

                var feeds = userBO.Feeds;
                await PerformRefreshOnFeeds(feeds);
            }
            else
                await LoadIndexOnly(userId, saveOnFail: true);

            var indexSubset = userIndex.FeedIndices;
            return CreateOutgoingFeedsInfoList(indexSubset, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, string category, bool refresh = false, bool nested = false)
        {
            if (refresh)
            {
                await LoadBoth(userId);

                var feeds = userBO.CreateSubsetFromCategory(category);
                await PerformRefreshOnFeeds(feeds);
            }
            else
                await LoadIndexOnly(userId, saveOnFail: true);

            var indexSubset = userIndex.FeedIndices.OfCategory(category);
            return CreateOutgoingFeedsInfoList(indexSubset, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, Guid feedId, bool refresh = false, bool nested = false)
        {
            if (refresh)
            {
                await LoadBoth(userId);

                var feeds = userBO.CreateSubsetFromFeedIds(new[] { feedId });
                await PerformRefreshOnFeeds(feeds);
            }
            else
                await LoadIndexOnly(userId, saveOnFail: true);

            var indexSubset = userIndex.FeedIndices.Where(o => o.Id == feedId);
            return CreateOutgoingFeedsInfoList(indexSubset, nested);
        }

        #endregion




        #region Feed management

        [HttpPost]
        [ActionName("add_feed")]
        public async Task<Outgoing.Feed> AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            await LoadIndexOnly(userId);

            var feedIndex = ConvertToFeedIndex(feed);
            if (userIndex.FeedIndices.Add(feedIndex))
            {
                await SaveUserIndex();
                return CreateOutgoingFeed(feedIndex);
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
            await LoadIndexOnly(userId);

            userIndex.FeedIndices.RemoveWithId(feedId);
            await SaveUserIndex();
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            await LoadIndexOnly(userId);

            var feedIndex = ConvertToFeedIndex(feed);
            userIndex.FeedIndices.Update(feedIndex);

            await SaveUserIndex();
        }

        [HttpPost]
        [ActionName("batch_change")]
        public async Task BatchChange(Guid userId, [FromBody] Incoming.BatchFeedChange changeSet)
        {
            if (changeSet == null)
                return;

            await LoadIndexOnly(userId);

            var added = changeSet.Added;
            var removed = changeSet.Removed;
            var updated = changeSet.Updated;

            if (!EnumerableEx.IsNullOrEmpty(added))
            {
                foreach (var feed in added)
                {
                    var feedIndex = ConvertToFeedIndex(feed);
                    userIndex.FeedIndices.Add(feedIndex);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(removed))
            {
                foreach (var feedId in removed)
                {
                    userIndex.FeedIndices.RemoveWithId(feedId);
                }
            }

            if (!EnumerableEx.IsNullOrEmpty(updated))
            {
                foreach (var feed in updated)
                {
                    var feedIndex = ConvertToFeedIndex(feed);
                    userIndex.FeedIndices.Update(feedIndex);
                }
            }

            await SaveUserIndex();
        }

        #endregion




        #region Article management

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.MarkRead(newsItemId);
            articleQueueService.QueueMarkRead(userId, newsItemId);

            await SaveUserIndex();
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.MarkUnread(newsItemId);
            articleQueueService.QueueMarkUnread(userId, newsItemId);

            await SaveUserIndex();
        }

        [HttpPost]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.MarkRead(newsItemIds);

            await SaveUserIndex();
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.MarkCategoryRead(category);

            await SaveUserIndex();
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.MarkFeedRead(feedId);

            await SaveUserIndex();
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.AddFavorite(newsItemId);
            articleQueueService.QueueAddFavorite(userId, newsItemId);

            await SaveUserIndex();
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            await LoadIndexOnly(userId);

            userIndex.Articles.RemoveFavorite(newsItemId);
            articleQueueService.QueueRemoveFavorite(userId, newsItemId);

            await SaveUserIndex();
        }

        #endregion




        #region Article Expiry times (Marked Read and Unread Deletion times)

        [HttpPost]
        [ActionName("set_delete_times")]
        public async Task SetArticleDeleteTimes(Guid userId, Incoming.ArticleDeleteTimes articleDeleteTimes)
        {
            await LoadIndexOnly(userId);

            userIndex.ArticleDeletionTimeForMarkedRead = articleDeleteTimes.ArticleDeletionTimeForMarkedRead;
            userIndex.ArticleDeletionTimeForUnread = articleDeleteTimes.ArticleDeletionTimeForUnread;

            await SaveUserIndex();
        }

        #endregion




        /// **************************************************************************
        /// ******************* END SERVICE-RELATED FUNCTIONS ************************
        /// **************************************************************************
        



        #region Load User graph and index functions

        /// <summary>
        /// Try to load the user index.  If that fails, try to load the full user graph
        /// from blob storage, then rehydrate the user index
        /// </summary>
        async Task LoadIndexOnly(Guid userId, bool saveOnFail = false)
        {
            this.userId = userId;
            VerifyUserId();

            bool wasIndexLoaded = false;

            try
            {
                await LoadUserIndex();
                wasIndexLoaded = true;
            }
            catch { }

            if (!wasIndexLoaded)
            {
                await LoadUserInfoBusinessObject();
                userIndex = userBO.CreateUserIndex();

                // save the news articles from the refreshed feeds to Redis
                var redisNews = userBO.Feeds.AllNews().Select(MapAsRedis);
                var saveArticlesResults = await newsItemCache.Set(redisNews);
                DebugEx.WriteLine(saveArticlesResults);

                // TODO: DETERMINE IF THIS SAVE IS SUPERFLUOUS, AND SHOULD BE DELAYED 
                // UNTIL THE INDEX HAS BEEN MODIFIED VIA WHICHEVER CONTROLLER ACTION 
                // WAS CALLED
                //if (saveOnFail)
                //{
                    await SaveUserIndex();
                //}
            }
        }

        /// <summary>
        /// Load the user Index first.  If the full user graph was not loaded yet 
        /// (which will be the majority case) then load it.  Finally, merge any changes 
        /// from the current user index state into the user graph.
        /// </summary>
        async Task LoadBoth(Guid userId)
        {
            await LoadIndexOnly(userId);

            if (userBO == null)
            {
                await LoadUserInfoBusinessObject();
            }

            userBO.UpdateFrom(userIndex);
        }

        void VerifyUserId()
        {
            if (userId == Guid.Empty)
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                    "You must specify a valid userId as a GUID");
        }

        async Task LoadUserIndex()
        {
            if (userId == Guid.Empty)
                throw new Exception("The user Id is set to the empty GUID, which should never be the case");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var result = await userIndexCache.Get(userId);

                if (!result.HasValue)
                    throw new Exception("unable to retrieve user matching the give id from the Redis cache");

                userIndex = result.Value;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sw.Stop();
                DebugEx.WriteLine("took {0} ms to get user index from cache", sw.ElapsedMilliseconds);
                readTime += sw.Elapsed;
            }
        }

        async Task LoadUserInfoBusinessObject()
        {
            if (userId == Guid.Empty)
                throw new Exception("The user Id is set to the empty GUID, which should never be the case");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                userBO = await userRepo.Get(userId);
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
                readTime += sw.Elapsed;
            }
        }

        #endregion




        #region Save user graph and index functions

        async Task SaveUserIndex()
        {
            if (userIndex == null)
                throw new Exception("User Index needs to be loaded/hydrated before you can save it");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var saveResult = await userIndexCache.Save(userIndex);
                DebugEx.WriteLine(saveResult);
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                throw;
            }
            finally
            {
                sw.Stop();
                writeTime += sw.Elapsed;
            }

            // TODO: Add code here that notifies some process that the UserBO needs to now be updated
        }

        void SaveUserInfoBusinessObject()
        {
            if (userBO == null)
                throw new Exception("User Business Object needs to be loaded/hydrated before you can save it");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                userRepo.Save(userId, userBO);
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                throw;
            }
            finally
            {
                sw.Stop();
                writeTime += sw.Elapsed;
            }
        }

        #endregion




        #region Feeds Refresh function

        async Task PerformRefreshOnFeeds(IEnumerable<Feed> feeds)
        {
            // refresh news for relevant feeds
            var subset = new FeedsSubset(feeds);
            await subset.Refresh();

            // recreate the userIndex from the newly updated user graph
            userIndex = userBO.CreateUserIndex();

            // save the news articles from the refreshed feeds to Redis
            var redisNews = subset.AllNews().Select(MapAsRedis);
            var saveToRedisResults = await newsItemCache.Set(redisNews);

            // save the user index, which was recreated earlier
            await SaveUserIndex();

            // save the full user graph, which was updated via the refresh
            SaveUserInfoBusinessObject();
        }

        #endregion




        #region Create "Get News" outgoing DTO

        class NewsItemIndexFeedIndexTuple
        {
            public NewsItemIndex NewsItemIndex { get; private set; }
            public FeedIndex FeedIndex { get; private set; }
            public bool IsNew { get; private set; }

            public NewsItemIndexFeedIndexTuple(NewsItemIndex newsItemIndex, FeedIndex feedIndex)
            {
                NewsItemIndex = newsItemIndex;
                FeedIndex = feedIndex;
                IsNew = feedIndex.IsNewsItemNew(newsItemIndex);
            }
        }

        async Task<Outgoing.NewsList> CreateNewsListFromSubset(
            IEnumerable<FeedIndex> feeds,
            int skip, 
            int take, 
            NewsItemType type, 
            EntryType entry,
            bool requireImage)
        {
            IEnumerable<NewsItemIndexFeedIndexTuple> indices = feeds
                .Where(o => o.NewsItemIndices != null)
                .SelectMany(o => o.NewsItemIndices.Select(x => new NewsItemIndexFeedIndexTuple(x, o)));

            if (type == NewsItemType.New)
                indices = indices.Where(o => o.IsNew);
            else if (type == NewsItemType.Viewed)
                indices = indices.Where(o => o.NewsItemIndex.HasBeenViewed);
            else if (type == NewsItemType.Unviewed)
                indices = indices.Where(o => !o.NewsItemIndex.HasBeenViewed);

            if (requireImage)
                indices = indices.Where(o => o.NewsItemIndex.HasImage);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            indices = indices
                .OrderByDescending(o => o.IsNew)
                .ThenByDescending(o => o.NewsItemIndex.UtcPublishDateTime)
                .Skip(skip)
                .Take(take)
                .ToList();
            sw.Stop();
            DebugEx.WriteLine("creating ordered indices took {0} ms", sw.ElapsedMilliseconds);

            var newsIds = indices.Select(o => o.NewsItemIndex.Id);

            sw.Restart();
            var newsItems = await newsItemCache.Get(newsIds);
            sw.Stop();
            DebugEx.WriteLine("getting newsItems from cache took {0} ms", sw.ElapsedMilliseconds);
            readTime += sw.Elapsed;

            var zipped = indices.Zip(newsItems, (tuple, ni) => new { tuple, ni });

            sw.Restart();
            var outgoingNews =
               (from temp in zipped.Where(o => o.ni.Value != null)
                let tuple = temp.tuple
                let newsItem = temp.ni.Value
                select Merge(tuple, newsItem)).ToList();
            sw.Stop();
            DebugEx.WriteLine("creating outgoing news took {0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var outgoingFeeds = feeds.Select(CreateOutgoingFeed).ToList();
            sw.Stop();
            DebugEx.WriteLine("creating outgoing feeds took {0} ms", sw.ElapsedMilliseconds);

            var outgoing = new Outgoing.NewsList
            {
                UserId = userId,
                FeedCount = outgoingFeeds.Count,
                Feeds = outgoingFeeds,
                News = outgoingNews,
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

        static Outgoing.Feed CreateOutgoingFeed(FeedIndex o)
        {
            return new Outgoing.Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                ArticleViewingType = (Weave.User.Service.DTOs.ArticleViewingType)o.ArticleViewingType,
                TeaserImageUrl = o.TeaserImageUrl,
                LastRefreshedOn = o.LastRefreshedOn,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
                NewArticleCount = o.NewsItemIndices.CountNew(),
                UnreadArticleCount = o.NewsItemIndices.CountUnread(),
                TotalArticleCount = o.NewsItemIndices.Count,
            };
        }

        static Outgoing.NewsItem Merge(NewsItemIndexFeedIndexTuple tuple, Redis.DTOs.NewsItem newsItem)
        {
            var newsIndex = tuple.NewsItemIndex;
            var feedIndex = tuple.FeedIndex;

            return new Outgoing.NewsItem
            {
                FeedId = feedIndex.Id,
                
                Id = newsItem.Id,
                Title = newsItem.Title,
                Link = newsItem.Link,
                UtcPublishDateTime = newsItem.UtcPublishDateTimeString,
                ImageUrl = newsItem.ImageUrl,
                YoutubeId = newsItem.YoutubeId,
                VideoUri = newsItem.VideoUri,
                PodcastUri = newsItem.PodcastUri,
                ZuneAppId = newsItem.ZuneAppId,
                Image = newsItem.Image == null ? null : MapToOutgoing(newsItem.Image),
                
                IsNew = tuple.IsNew,

                IsFavorite = newsIndex.IsFavorite,
                HasBeenViewed = newsIndex.HasBeenViewed,
                OriginalDownloadDateTime = newsIndex.OriginalDownloadDateTime,
            };
        }

        static Outgoing.Image MapToOutgoing(Redis.DTOs.Image o)
        {
            return new Outgoing.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        #endregion




        #region Create "Feeds List" outgoing DTO

        Outgoing.FeedsInfoList CreateOutgoingFeedsInfoList(IEnumerable<FeedIndex> feeds, bool returnNested)
        {
            Outgoing.FeedsInfoList outgoing = new Outgoing.FeedsInfoList
            {
                UserId = userId,
                TotalFeedCount = userIndex.FeedIndices == null ? 0 : userIndex.FeedIndices.Count(),
            };

            if (EnumerableEx.IsNullOrEmpty(feeds))
            {
                outgoing.NewArticleCount = 0;
                outgoing.UnreadArticleCount = 0;
                outgoing.TotalArticleCount = 0;
                return outgoing;
            }

            var outgoingFeeds = feeds.Select(CreateOutgoingFeed).ToList();
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

        static Outgoing.CategoryInfo CreateCategory(string categoryName, List<Outgoing.Feed> feeds)
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

        #endregion




        #region Conversion Helpers

        UserInfo ConvertToBusinessObject(Incoming.UserInfo user)
        {
            return user.Convert<Incoming.UserInfo, UserInfo>(ServerIncomingToBusinessObject.Instance);
        }

        FeedIndex ConvertToFeedIndex(Incoming.NewFeed o)
        {
            return new FeedIndex
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (BusinessObjects.ArticleViewingType)o.ArticleViewingType,
            };
        }

        FeedIndex ConvertToFeedIndex(Incoming.UpdatedFeed feed)
        {
            return new FeedIndex
            {
                Id = feed.Id,
                Name = feed.Name,
                Category = feed.Category,
                ArticleViewingType = (BusinessObjects.ArticleViewingType)feed.ArticleViewingType,
            };
        }
           
        //Outgoing.NewsItem ConvertToOutgoing(NewsItem user)
        //{
        //    return user.Convert<NewsItem, Outgoing.NewsItem>(BusinessObjectToServerOutgoing.Instance);
        //}

        Outgoing.UserInfo ConvertToOutgoing(UserIndex user)
        {
            return user.Convert<UserIndex, Outgoing.UserInfo>(BusinessObjectToServerOutgoing.Instance);
        }

        Redis.DTOs.NewsItem MapAsRedis(NewsItem o)
        {
            return new Redis.DTOs.NewsItem
            {
                Id = o.Id,
                UtcPublishDateTimeString = o.UtcPublishDateTimeString,
                UtcPublishDateTime = o.UtcPublishDateTime,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Image = o.Image == null ? null : MapAsRedis(o.Image),
            };
        }

        Redis.DTOs.Image MapAsRedis(Image o)
        {
            return new Redis.DTOs.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        #endregion
    }
}