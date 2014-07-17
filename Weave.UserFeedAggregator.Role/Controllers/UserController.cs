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
    public interface IArticleQueueService
    {
        void QueueMarkRead(Guid userId, Guid newsItemId);
        void QueueMarkUnread(Guid userId, Guid newsItemId);
        void QueueAddFavorite(Guid userId, Guid newsItemId);
        void QueueRemoveFavorite(Guid userId, Guid newsItemId);
    }

    public class UserController : ApiController, IWeaveUserService
    {
        UserInfo userBO;
        UserRepository userRepo;
        UserIndex userIndex;
        IArticleQueueService articleQueueService;
        NewsItemCache newsItemCache;

        TimeSpan readTime = TimeSpan.Zero, writeTime = TimeSpan.Zero;

        public UserController(
            IArticleQueueService articleQueueService)
        {
            this.articleQueueService = articleQueueService;
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
            SaveUserIndex();

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

            category = category ?? "all news";

            var feeds = userIndex.FeedIndices.Where(o => MatchesCategory(o, category)).ToList();

            if (entry == EntryType.Mark || entry == EntryType.ExtendRefresh)
            {
                if (entry == EntryType.Mark)
                {
                    feeds.MarkEntry();
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    feeds.ExtendEntry();

                    // compare new feeds on every fullFeed with feed, add new news items to the newsItemCache, as well as the FeedIndex
                    //await subset.Refresh();
                }

                //userBO.DeleteOldNews();
                SaveUserIndex();
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

        bool MatchesCategory(FeedIndex feed, string category)
        {
            if ("all news".Equals(category, StringComparison.OrdinalIgnoreCase))
                return true;

            return category.Equals(feed.Category, StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.NewsList> GetNews(
            Guid userId, Guid feedId, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            await VerifyUserId(userId);

            var feeds = userIndex.FeedIndices.Where(o => o.Id == feedId).ToList();

            if (entry == EntryType.Mark || entry == EntryType.ExtendRefresh)
            {
                if (entry == EntryType.Mark)
                {
                    feeds.MarkEntry();
                }

                else if (entry == EntryType.ExtendRefresh)
                {
                    feeds.ExtendEntry();
                    //await subset.Refresh();
                }

                userBO.DeleteOldNews();
                SaveUserIndex();
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
            await VerifyUserId(userId);

            if (refresh)
            {
                await userBO.RefreshAllFeeds();
                SaveUserIndex();
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
                SaveUserIndex();
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
                SaveUserIndex();
            }
            return CreateOutgoingFeedsInfoList(subset, nested);
        }

        #endregion




        #region Feed management

        [HttpPost]
        [ActionName("add_feed")]
        public async Task<Outgoing.Feed> AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            await VerifyUserId(userId);

            var feedIndex = ConvertToFeedIndex(feed);
            if (userIndex.FeedIndices.TryAdd(feedIndex))
            {
                SaveUserIndex();
                return ConvertToOutgoing(feedIndex);
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

            userIndex.FeedIndices.RemoveWithId(feedId);
            SaveUserIndex();
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            await VerifyUserId(userId);

            var feedIndex = ConvertToFeedIndex(feed);
            userIndex.FeedIndices.Update(feedIndex);

            SaveUserIndex();
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
                    var feedIndex = ConvertToFeedIndex(feed);
                    userIndex.FeedIndices.TryAdd(feedIndex);
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

            SaveUserIndex();
        }

        #endregion




        #region Article management

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            userIndex.Articles.MarkRead(newsItemId);
            articleQueueService.QueueMarkRead(userId, newsItemId);

            SaveUserIndex();
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            userIndex.Articles.MarkUnread(newsItemId);
            articleQueueService.QueueMarkUnread(userId, newsItemId);

            SaveUserIndex();
        }

        [HttpPost]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, [FromBody] List<Guid> newsItemIds)
        {
            await VerifyUserId(userId);

            userIndex.Articles.MarkRead(newsItemIds);

            SaveUserIndex();
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            await VerifyUserId(userId);

            userIndex.Articles.MarkCategoryRead(category);

            SaveUserIndex();
        }

        [HttpGet]
        [ActionName("soft_read")]
        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            await VerifyUserId(userId);

            userIndex.Articles.MarkFeedRead(feedId);

            SaveUserIndex();
        }

        [HttpGet]
        [ActionName("add_favorite")]
        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            userIndex.Articles.AddFavorite(newsItemId);
            articleQueueService.QueueAddFavorite(userId, newsItemId);

            SaveUserIndex();
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            await VerifyUserId(userId);

            userIndex.Articles.RemoveFavorite(newsItemId);
            articleQueueService.QueueRemoveFavorite(userId, newsItemId);

            SaveUserIndex();
        }

        #endregion




        #region Article Expiry times (Marked Read and Unread Deletion times)

        [HttpPost]
        [ActionName("set_delete_times")]
        public async Task SetArticleDeleteTimes(Guid userId, Incoming.ArticleDeleteTimes articleDeleteTimes)
        {
            await VerifyUserId(userId);

            userIndex.ArticleDeletionTimeForMarkedRead = articleDeleteTimes.ArticleDeletionTimeForMarkedRead;
            userIndex.ArticleDeletionTimeForUnread = articleDeleteTimes.ArticleDeletionTimeForUnread;
            
            SaveUserIndex();
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



        async Task<Outgoing.NewsList> CreateNewsListFromSubset(
            IEnumerable<FeedIndex> feeds,
            int skip, 
            int take, 
            NewsItemType type, 
            EntryType entry,
            bool requireImage)
        {
            var indices = feeds.Ordered().ToArray();

            IEnumerable<NewsItemIndex> filteredIndices = indices;

            if (type == NewsItemType.New) ;
            //    filteredIndices = filteredIndices.Where(o => o.IsNew()).ToList();

            else if (type == NewsItemType.Viewed)
                filteredIndices = filteredIndices.Where(o => o.HasBeenViewed);

            else if (type == NewsItemType.Unviewed)
                filteredIndices = filteredIndices.Where(o => !o.HasBeenViewed);

            if (requireImage)
                filteredIndices = filteredIndices.Where(o => o.HasImage);

            filteredIndices = filteredIndices.Skip(skip).Take(take);

            var newsItems = await newsItemCache.Get(indices.Select(o => o.Id));

            var zipped = filteredIndices.Zip(newsItems, (index, ni) => new { index, ni });

            var results =
                from temp in zipped.Where(o => o.ni.Value != null)
                let newsIndex = temp.index
                let newsItem = temp.ni.Value
                select Merge(newsIndex, newsItem);

            var outgoing = new Outgoing.NewsList
            {
                UserId = userIndex.Id,
                FeedCount = feeds.Count(),
                Feeds = feeds.Select(ConvertToOutgoing).ToList(),
                News = results.Select(ConvertToOutgoing).ToList(),
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

        static NewsItem Merge(NewsItemIndex newsIndex, Weave.User.Service.Redis.DTOs.NewsItem newsItem)
        {
            return new NewsItem
            {
                Id = newsItem.Id,
                Title = newsItem.Title,
                Link = newsItem.Link,
                ImageUrl = newsItem.ImageUrl,
                YoutubeId = newsItem.YoutubeId,
                VideoUri = newsItem.VideoUri,
                PodcastUri = newsItem.PodcastUri,
                ZuneAppId = newsItem.ZuneAppId,
                IsFavorite = newsIndex.IsFavorite,
                HasBeenViewed = newsIndex.HasBeenViewed,
                OriginalDownloadDateTime = newsItem.OriginalDownloadDateTime,
                UtcPublishDateTimeString = newsItem.UtcPublishDateTime,
                Image = newsItem.Image == null ? null :
                    new Image
                    {
                        Width = newsItem.Image.Width,
                        Height = newsItem.Image.Height,
                        OriginalUrl = newsItem.Image.OriginalUrl,
                        BaseImageUrl = newsItem.Image.BaseImageUrl,
                        SupportedFormats = newsItem.Image.SupportedFormats,
                    }
            };
        }

        void SaveUserIndex()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                userRepo.Save(userBO.Id, userBO);
            }
            catch(Exception e)
            {
                DebugEx.WriteLine(e);
                throw;
            }
            finally
            {
                sw.Stop();
                writeTime = sw.Elapsed;
            }

// TODO: Add code here that notifies some process that the UserBO needs to now be updated
        }

        #endregion




        #region Conversion Helpers

        Feed Convert(FeedIndex feed)
        {
            return new Feed
            {

            };
        }

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
                Category = feed.Category
                ArticleViewingType = (BusinessObjects.ArticleViewingType)feed.ArticleViewingType,
            };
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

        Outgoing.Feed ConvertToOutgoing(FeedIndex o)
        {
            return new Outgoing.Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                ArticleViewingType = (Weave.User.Service.DTOs.ArticleViewingType)o.ArticleViewingType,
                //NewArticleCount = o.NewArticleCount,
                //UnreadArticleCount = o.UnreadArticleCount,
                //TotalArticleCount = o.TotalArticleCount,
                TeaserImageUrl = o.TeaserImageUrl,
                LastRefreshedOn = o.LastRefreshedOn,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };
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