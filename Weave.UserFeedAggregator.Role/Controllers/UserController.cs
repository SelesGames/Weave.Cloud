using SelesGames.WebApi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Contracts;
using Weave.User.Service.DTOs;
using Weave.User.Service.InterRoleMessaging.Articles;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Synchronization.UserIndex;
using Weave.User.Service.Role.Map;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;

namespace Weave.User.Service.Role.Controllers
{
    public class UserController : ApiController
    {
        #region Private member variables + constructor

        readonly ConnectionMultiplexer connection;
        readonly Weave.User.BusinessObjects.Mutable.Cache.UserIndexCache userIndexCache;
        readonly UserLockHelper userLockHelper;
        readonly IArticleQueueService articleQueueService;

        dynamic timings;

        Guid userId;
        UserIndex userIndex;

        public UserController(
            ConnectionMultiplexer connection,
            Weave.User.BusinessObjects.Mutable.Cache.UserIndexCache userIndexCache,
            UserLockHelper userLockHelper,
            IArticleQueueService articleQueueService)
        {
            this.connection = connection;
            this.userIndexCache = userIndexCache;
            this.userLockHelper = userLockHelper;
            this.articleQueueService = articleQueueService;

            timings = new System.Dynamic.ExpandoObject();
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
                    userIndex = await userIndexCache.Get(incomingUser.Id);
                    //userBO = await userRepo.Get(incomingUser.Id);
                    if (userIndex != null)
                        doesUserAlreadyExist = true;
                }
                finally
                {
                    sw.Stop();
                    timings.AddUserAndReturnUserInfo_TryGetUser = sw.Elapsed.Dump();
                }
            }

            if (doesUserAlreadyExist)
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "A user with that Id already exists");
            }

            userIndex = ServerIncomingToBusinessObject.Convert(incomingUser);
            this.userId = userIndex.Id;
            await SaveUserIndex();
            await PerformRefreshOnFeeds(userIndex.FeedIndices);

            var outgoing = ConvertToOutgoing(userIndex);
            outgoing.Timings = timings;
            return outgoing;
        }

        #endregion




        #region Get Basic User Info (suitable for panorama home screen)

        [HttpGet]
        [ActionName("info")]
        public async Task<Outgoing.UserInfo> GetUserInfo(Guid userId, bool refresh = false)
        {
            // Modify the user data to record a "login", and optionally refresh all their feed indices
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                userIndex.PreviousLoginTime = userIndex.CurrentLoginTime;
                userIndex.CurrentLoginTime = DateTime.UtcNow;

                if (refresh)
                {
                    await PerformRefreshOnFeeds(userIndex.FeedIndices);
                    DeleteOldNews(userIndex.FeedIndices);
                }

                await SaveUserIndex();
            });


            var outgoing = ConvertToOutgoing(userIndex);
            var latestNewsIndices = userIndex.FeedIndices.GetLatestNews();
            var latestNews = await CreateOutgoingNews(latestNewsIndices);
            outgoing.LatestNews = latestNews;

            outgoing.Timings = timings;

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
            category = category ?? "all news";

            return await GetNewsImpl(
                userId: userId,
                feedSelector: o => o.FeedIndices.OfCategory(category).ToList(),
                skip: skip,
                take: take,
                type: type,
                entry: entry,
                requireImage: requireImage);
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, Guid feedId, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            return await GetNewsImpl(
                userId: userId,
                feedSelector: o => o.FeedIndices.WithId(feedId).ToList(),
                skip: skip,
                take: take,
                type: type,
                entry: entry,
                requireImage: requireImage);
        }

        async Task<Outgoing.NewsList> GetNewsImpl(
            Guid userId, 
            Func<UserIndex, IEnumerable<FeedIndex>> feedSelector, 
            EntryType entry, 
            int skip, 
            int take, 
            NewsItemType type, 
            bool requireImage)
        {
            IEnumerable<FeedIndex> feeds = null;

            if (entry == EntryType.ExtendRefresh)
            {
                await Lock(userId, async () =>
                {
                    await LoadIndexOnly(userId);
                    feeds = feedSelector(userIndex);
                    feeds.ExtendEntry();
                    await PerformRefreshOnFeeds(feeds);
                    await SaveUserIndex();
                });
            }
            else if (entry == EntryType.Mark)
            {
                await Lock(userId, async () =>
                {
                    await LoadIndexOnly(userId);
                    feeds = feedSelector(userIndex);
                    feeds.MarkEntry();
                    await SaveUserIndex();
                });
            }
            else
            {
                await LoadIndexOnly(userId);
                feeds = feedSelector(userIndex);
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
                await Lock(userId, async () =>
                {
                    await LoadIndexOnly(userId);
                    var feeds = userIndex.FeedIndices;
                    await PerformRefreshOnFeeds(feeds);
                    await SaveUserIndex();
                });
            }
            else
                await LoadIndexOnly(userId);

            var indexSubset = userIndex.FeedIndices;
            return CreateOutgoingFeedsInfoList(indexSubset, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, string category, bool refresh = false, bool nested = false)
        {
            if (refresh)
            {
                await Lock(userId, async () =>
                {
                    await LoadIndexOnly(userId);
                    var feeds = userIndex.FeedIndices.OfCategory(category);
                    await PerformRefreshOnFeeds(feeds);
                    await SaveUserIndex();
                });
            }
            else
                await LoadIndexOnly(userId);

            var indexSubset = userIndex.FeedIndices.OfCategory(category);
            return CreateOutgoingFeedsInfoList(indexSubset, nested);
        }

        [HttpGet]
        [ActionName("feeds")]
        public async Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, Guid feedId, bool refresh = false, bool nested = false)
        {
            if (refresh)
            {
                await Lock(userId, async () =>
                {
                    await LoadIndexOnly(userId);
                    var feeds = userIndex.FeedIndices.WithId(feedId);
                    await PerformRefreshOnFeeds(feeds);
                    await SaveUserIndex();
                });
            }
            else
                await LoadIndexOnly(userId);

            var indexSubset = userIndex.FeedIndices.Where(o => o.Id == feedId);
            return CreateOutgoingFeedsInfoList(indexSubset, nested);
        }

        #endregion




        #region Feed management

        [HttpPost]
        [ActionName("add_feed")]
        public async Task<Outgoing.Feed> AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            return await Lock(userId, async () =>
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
            });
        }

        [HttpGet]
        [ActionName("remove_feed")]
        public async Task RemoveFeed(Guid userId, Guid feedId)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                userIndex.FeedIndices.RemoveWithId(feedId);

                await SaveUserIndex();
            });
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var feedIndex = ConvertToFeedIndex(feed);
                userIndex.FeedIndices.Update(feedIndex);

                await SaveUserIndex();
            });
        }

        [HttpPost]
        [ActionName("batch_change")]
        public async Task BatchChange(Guid userId, [FromBody] Incoming.BatchFeedChange changeSet)
        {
            if (changeSet == null)
                return;

            await Lock(userId, async () =>
            {
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
            });
        }

        #endregion




        #region Article management

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var article = userIndex.Articles.MarkRead(newsItemId);
                if (article != null)
                {
                    articleQueueService.QueueMarkRead(userId, newsItemId, article.FeedIndex.Name);
                    await SaveUserIndex();
                }
            });
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var article = userIndex.Articles.MarkUnread(newsItemId);
                if (article != null)
                {
                    articleQueueService.QueueMarkUnread(userId, newsItemId, article.FeedIndex.Name);
                    await SaveUserIndex();
                }
            });
        }

        [HttpPost]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var articles = userIndex.Articles.MarkRead(newsItemIds);
                if (articles.Any())
                {
                    await SaveUserIndex();
                }
            });
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var articles = userIndex.Articles.MarkCategoryRead(category);
                if (articles.Any())
                {
                    await SaveUserIndex();
                }
            });
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var articles = userIndex.Articles.MarkFeedRead(feedId);
                if (articles.Any())
                {
                    await SaveUserIndex();
                }
            });
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var article = userIndex.Articles.AddFavorite(newsItemId);
                if (article != null)
                {
                    articleQueueService.QueueAddFavorite(userId, newsItemId, article.FeedIndex.Name);
                    await SaveUserIndex();
                }
            });
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                var article = userIndex.Articles.RemoveFavorite(newsItemId);
                if (article != null)
                {
                    articleQueueService.QueueRemoveFavorite(userId, newsItemId, article.FeedIndex.Name);
                    await SaveUserIndex();
                }
            });
        }

        #endregion




        #region Article Expiry times (Marked Read and Unread Deletion times)

        [HttpPost]
        [ActionName("set_delete_times")]
        public async Task SetArticleDeleteTimes(Guid userId, Incoming.ArticleDeleteTimes articleDeleteTimes)
        {
            await Lock(userId, async () =>
            {
                await LoadIndexOnly(userId);

                userIndex.ArticleDeletionTimeForMarkedRead = articleDeleteTimes.ArticleDeletionTimeForMarkedRead;
                userIndex.ArticleDeletionTimeForUnread = articleDeleteTimes.ArticleDeletionTimeForUnread;

                await SaveUserIndex();
            });
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
        async Task LoadIndexOnly(Guid userId)
        {
            this.userId = userId;
            VerifyUserId();
            await LoadUserIndex();
        }

        void VerifyUserId()
        {
            if (userId == Guid.Empty)
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                    "You must specify a valid userId as a GUID");
        }

        async Task LoadUserIndex()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var result = await userIndexCache.Get(userId);

                if (result == null)
                    throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound,
                        "unable to retrieve user matching the give id from the user cache");

                userIndex = result;
            }
            finally
            {
                sw.Stop();
                DebugEx.WriteLine("took {0} ms to get user index from cache", sw.ElapsedMilliseconds);
                timings.LoadUserIndex = sw.Elapsed.Dump();
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
                timings.SaveUserIndex = sw.Elapsed.Dump();
            }
        }

        #endregion




        #region Feeds Refresh function

        async Task PerformRefreshOnFeeds(IEnumerable<FeedIndex> feeds)
        {
            var updateHelper = new UpdateHelper(connection);
            var refreshMeta = await updateHelper.PerformRefreshOnFeeds(feeds);
            timings.RefreshMeta = refreshMeta;
        }

        //********** test
        public void DeleteOldNews(IEnumerable<FeedIndex> feeds)
        {
            var now = DateTime.UtcNow;

            foreach (var feed in feeds)
            {
                var toDelete = feed.NewsItemIndices.Where(o => IsNewsOld(o, now)).ToList();
                foreach (var newsItemIndex in toDelete)
                    feed.NewsItemIndices.Remove(newsItemIndex);
            }
        }

        bool IsNewsOld(NewsItemIndex newsItem, DateTime now)
        {
            var age = now - newsItem.UtcPublishDateTime;

            var isNew =
                newsItem.IsFavorite ||
                newsItem.FeedIndex.IsNewsItemNew(newsItem) ||
                (!newsItem.HasBeenViewed && (age < userIndex.ArticleDeletionTimeForUnread)) ||
                (newsItem.HasBeenViewed && (age < userIndex.ArticleDeletionTimeForMarkedRead));

            return !isNew;
        }
        //***************

        #endregion




        #region Create "Get News" outgoing DTO

        async Task<Outgoing.NewsList> CreateNewsListFromSubset(
            IEnumerable<FeedIndex> feeds,
            int skip, 
            int take, 
            NewsItemType type, 
            EntryType entry,
            bool requireImage)
        {
            feeds = feeds ?? new List<FeedIndex>(0);

            var indices = feeds.AllIndices();

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
                .Ordered()
                .Skip(skip)
                .Take(take)
                .ToList();
            sw.Stop();
            timings.CreateOrderedNewsIndices = sw.Elapsed.Dump();
            //DebugEx.WriteLine("creating ordered indices took {0} ms", sw.ElapsedMilliseconds);

            var outgoingNews = await CreateOutgoingNews(indices);
            
            sw.Restart();
            var outgoingFeeds = feeds.Select(CreateOutgoingFeed).ToList();
            sw.Stop();
            timings.CreateOutgoingFeeds = sw.Elapsed.Dump();
            //DebugEx.WriteLine("creating outgoing feeds took {0} ms", sw.ElapsedMilliseconds);

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

            outgoing.Timings = timings;

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
                //LastRefreshedOn = o.LastRefreshedOn,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
                NewArticleCount = o.NewsItemIndices.CountNew(),
                UnreadArticleCount = o.NewsItemIndices.CountUnread(),
                TotalArticleCount = o.NewsItemIndices.Count,
            };
        }

        async Task<List<Outgoing.NewsItem>> CreateOutgoingNews(IEnumerable<NewsItemIndexFeedIndexTuple> indices)
        {
            // no point in doing the work if no indices were passed in
            if (!indices.Any())
                return new List<Outgoing.NewsItem>();

            var newsIds = indices.Select(o => o.NewsItemIndex.Id);
            var newsItems = await GetNewsFromRedis(newsIds);

            var zipped = indices.Zip(newsItems, (tuple, ni) => new { tuple, ni });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var outgoingNews =
               (from temp in zipped.Where(o => o.ni.HasValue)
                let tuple = temp.tuple
                let entry = temp.ni.Value
                select Merge(tuple, entry)).ToList();
            sw.Stop();
            timings.CreateOutgoingNews = sw.Elapsed.Dump();

            return outgoingNews;
        }

        static Outgoing.NewsItem Merge(NewsItemIndexFeedIndexTuple tuple, ExpandedEntry entry)
        {
            var newsIndex = tuple.NewsItemIndex;
            var feedIndex = tuple.FeedIndex;

            var bestImage = entry.Images.GetBest();

            return new Outgoing.NewsItem
            {
                FeedId = feedIndex.Id,
                
                Id = entry.Id,
                Title = entry.Title,
                Link = entry.Link,
                UtcPublishDateTime = entry.UtcPublishDateTimeString,
                ImageUrl = bestImage == null ? null : bestImage.Url,
                YoutubeId = entry.YoutubeId,
                VideoUri = entry.VideoUri,
                PodcastUri = entry.PodcastUri,
                ZuneAppId = entry.ZuneAppId,
                Image = bestImage == null ? null : MapToOutgoing(bestImage),
                
                IsNew = tuple.IsNew,

                IsFavorite = newsIndex.IsFavorite,
                HasBeenViewed = newsIndex.HasBeenViewed,
                OriginalDownloadDateTime = newsIndex.OriginalDownloadDateTime,
            };
        }

        static Outgoing.Image MapToOutgoing(Updater.BusinessObjects.Image o)
        {
            return new Outgoing.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.Url,
                BaseImageUrl = null,
                SupportedFormats = null,
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

        FeedIndex ConvertToFeedIndex(Incoming.NewFeed o)
        {
            return new FeedIndex
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (Weave.User.BusinessObjects.Mutable.ArticleViewingType)o.ArticleViewingType,
            };
        }

        FeedIndex ConvertToFeedIndex(Incoming.UpdatedFeed feed)
        {
            return new FeedIndex
            {
                Id = feed.Id,
                Name = feed.Name,
                Category = feed.Category,
                ArticleViewingType = (Weave.User.BusinessObjects.Mutable.ArticleViewingType)feed.ArticleViewingType,
            };
        }

        Outgoing.UserInfo ConvertToOutgoing(UserIndex user)
        {
            return BusinessObjectToServerOutgoing.Convert(user);
        }

        #endregion




        #region Redis Lock/Unlock User Index

        Task<T> Lock<T>(Guid userId, Func<Task<T>> func)
        {
            return userLockHelper.Lock(userId, func);
        }

        Task Lock(Guid userId, Func<Task> func)
        {
            return userLockHelper.Lock(userId, func);
        }

        #endregion




        #region Redis News Cache interactions

        /// <summary>
        /// Get news from Redis, do not use batching
        /// </summary>
        async Task<IEnumerable<RedisCacheResult<ExpandedEntry>>> GetNewsFromRedis(IEnumerable<Guid> newsIds)
        {
            var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var cache = new ExpandedEntryCache(db);
            var entries = await cache.Get(newsIds);

            timings.GetNewsItemsFromCache_Deserialization = entries.Timings.SerializationTime.Dump();
            timings.GetNewsItemsFromCache_Retrieval = entries.Timings.ServiceTime.Dump();

            return entries.Results;
        }

        ///// <summary>
        ///// Save news to Redis; uses batching
        ///// </summary>
        //public Task<IEnumerable<bool>> SaveNewsToRedis(IEnumerable<Redis.DTOs.NewsItem> newsItems)
        //{
        //    var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
        //    var batch = db.CreateBatch();
        //    var newsItemCache = new NewsItemCache(batch);
        //    var resultsTask = newsItemCache.Set(newsItems);
        //    batch.Execute();
        //    return resultsTask;
        //}

        #endregion
    }

    static class TimeSpanFormattingExtensions
    {
        public static string Dump(this TimeSpan t)
        {
            return t.TotalMilliseconds + " ms";
        }
    }
}











#region deprecated

///// <summary>
///// Load the user Index first.  If the full user graph was not loaded yet 
///// (which will be the majority case) then load it.  Finally, merge any changes 
///// from the current user index state into the user graph.
///// </summary>
//async Task LoadBoth(Guid userId)
//{
//    await LoadIndexOnly(userId);

//    if (userBO == null)
//    {
//        await LoadUserInfoBusinessObject();
//        await LoadUserIndex();
//    }

//    userBO.UpdateFrom(userIndex);
//}

//async Task PerformRefreshOnFeeds(IEnumerable<Feed> feeds)
//{
//    // refresh news for relevant feeds
//    var subset = new FeedsSubset(feeds);

//    var sw = System.Diagnostics.Stopwatch.StartNew();
//    await subset.Refresh();
//    sw.Stop();
//    timings.Refresh = sw.Elapsed.Dump();

//    // 1. Lock the userIndex first, because we will be modifying/recreating it
//    // 2. Grab the latest user index, which may be different from the previous one
//    // that was grabbed prior to the refresh
//    // 3. Merge the state of the latest user index into the object graph
//    // 4. Recreate the userIndex from the newly updated user graph
//    // 5. Save the user index
//    await Lock(userId, async () =>
//    {
//        await LoadUserIndex();
//        userBO.UpdateFrom(userIndex);
//        userIndex = userBO.CreateUserIndex();
//        await SaveUserIndex();
//    });

//    // save the full user graph, which was updated via the refresh
//    SaveUserInfoBusinessObject();

//    // save the news articles from the refreshed feeds to Redis
//    var redisNews = subset.AllNews().Select(MapAsRedis);

//    sw.Restart();
//    var saveToRedisResults = await SaveNewsToRedis(redisNews);
//    sw.Stop();
//    timings.SaveToNewsCache = sw.Elapsed.Dump();
//}

#endregion