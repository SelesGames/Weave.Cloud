using SelesGames.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.UserFeedAggregator.BusinessObjects;
using Weave.UserFeedAggregator.Repositories;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;

namespace Weave.UserFeedAggregator.Role.Controllers
{
    public class UserController : ApiController
    {
        UserRepository userRepo;

        public UserController(UserRepository userRepo)
        {
            this.userRepo = userRepo;
        }




        #region User creation

        [HttpPost]
        [ActionName("create")]
        public async Task<Outgoing.UserInfo> AddUserAndReturnUserInfo([FromBody] Incoming.UserInfo incomingUser)
        {
            var userBO = ConvertToBusinessObject(incomingUser);
            var user = ConvertToDataStore(userBO);
            //await userRepo.Add(user);
            await userRepo.Save(user);
            await userBO.RefreshAllFeeds();
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
            var outgoing = ConvertToOutgoing(userBO);
            return outgoing;
        }

        #endregion




        #region Get Basic User Info (suitable for panorama home screen)

        [HttpGet]
        [ActionName("info")]
        public async Task<Outgoing.UserInfo> GetUserInfo(Guid userId, bool refresh = false)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);

            if (refresh)
            {
                await userBO.RefreshAllFeeds();
                user = ConvertToDataStore(userBO);
                await userRepo.Save(user);
            }

            var outgoing = ConvertToOutgoing(userBO);
            return outgoing;
        }

        #endregion




        #region Get News for User (either by category or feedId)

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.UserNews> GetNews(Guid userId, string category, bool refresh = false, int skip = 0, int take = 10)
        {
            TimeSpan readTime, writeTime = TimeSpan.Zero;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = await userRepo.Get(userId);
            sw.Stop();
            readTime = sw.Elapsed;

            var userBO = ConvertToBusinessObject(user);
            var subset = userBO.CreateSubsetFromCategory(category);

            if (refresh)
            {
                await subset.Refresh();
                user = ConvertToDataStore(userBO);

                sw = System.Diagnostics.Stopwatch.StartNew();
                await userRepo.Save(user);
                sw.Stop();
                writeTime = sw.Elapsed;
            }

            var outgoing = new Outgoing.UserNews
            {
                Id = userId,
                FeedCount = subset.Count(),
                Take = take,
                Skip = skip,
                Feeds = subset.Select(ConvertToOutgoing).ToList(),
                News = subset.AllOrderedNews().Select(ConvertToOutgoing).ToList(),//.Skip(skip).Take(take).ToList(),
                DataStoreReadTime = readTime,
                DataStoreWriteTime = writeTime,
            };
            outgoing.NewsCount = outgoing.News.Count;
            outgoing.HasMore = outgoing.NewsCount < take;

            return outgoing;
        }

        [HttpGet]
        [ActionName("news")]
        public async Task<Outgoing.UserNews> GetNews(Guid userId, Guid feedId, bool refresh = false, int skip = 0, int take = 10)
        {
            TimeSpan readTime, writeTime = TimeSpan.Zero;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = await userRepo.Get(userId);
            sw.Stop();
            readTime = sw.Elapsed;

            var userBO = ConvertToBusinessObject(user);
            var subset = userBO.CreateSubsetFromFeedIds(new[] { feedId });

            if (refresh)
            {
                await subset.Refresh();
                user = ConvertToDataStore(userBO);

                sw = System.Diagnostics.Stopwatch.StartNew();
                await userRepo.Save(user);
                sw.Stop();
                writeTime = sw.Elapsed;
            }

            var outgoing = new Outgoing.UserNews
            {
                Id = userId,
                FeedCount = subset.Count(),
                Take = take,
                Skip = skip,
                Feeds = subset.Select(ConvertToOutgoing).ToList(),
                News = subset.AllOrderedNews().Select(ConvertToOutgoing).ToList(),//.Skip(skip).Take(take).ToList(),
                DataStoreReadTime = readTime,
                DataStoreWriteTime = writeTime,
            };
            outgoing.NewsCount = outgoing.News.Count;
            outgoing.HasMore = outgoing.NewsCount < take;

            return outgoing;
        }

        [HttpGet]
        [ActionName("refresh_all")]
        public async Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId)
        {
            TimeSpan readTime, writeTime;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = await userRepo.Get(userId);
            sw.Stop();
            readTime = sw.Elapsed;

            var userBO = ConvertToBusinessObject(user);
            await userBO.RefreshAllFeeds();
            user = ConvertToDataStore(userBO);

            sw = System.Diagnostics.Stopwatch.StartNew();
            await userRepo.Save(user);
            sw.Stop();
            writeTime = sw.Elapsed;

            var outgoing = ConvertToOutgoing(userBO);

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;
            return outgoing;
        }

        /// <summary>
        /// Refresh only some of the feeds for a given user, then return the full UserInfo graph.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="feedIds">The ids of the feeds to refresh</param>
        /// <returns>UserInfo graph</returns>
        //[HttpPost]
        //[ActionName("refresh")]
        //public async Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId, [FromBody] List<Guid> feedIds)
        //{
        //    TimeSpan readTime, writeTime;

        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    var user = await userRepo.Get(userId);
        //    sw.Stop();
        //    readTime = sw.Elapsed;

        //    var userBO = ConvertToBusinessObject(user);
        //    await userBO.RefreshFeedsMatchingIds(feedIds);
        //    user = ConvertToDataStore(userBO);

        //    sw = System.Diagnostics.Stopwatch.StartNew();
        //    await userRepo.Save(user);
        //    sw.Stop();
        //    writeTime = sw.Elapsed;

        //    userBO = ConvertToBusinessObject(user);
        //    var outgoing = ConvertToOutgoing(userBO);

        //    outgoing.DataStoreReadTime = readTime;
        //    outgoing.DataStoreWriteTime = writeTime;
        //    return outgoing;
        //}



        #endregion




        #region Feed management

        [HttpPost]
        [ActionName("add_feed")]
        public async Task AddFeed(Guid userId, [FromBody] Incoming.NewFeed feed)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);
            var feedBO = ConvertToBusinessObject(feed);
            userBO.AddFeed(feedBO);
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
        }

        [HttpGet]
        [ActionName("remove_feed")]
        public async Task RemoveFeed(Guid userId, Guid feedId)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);
            userBO.RemoveFeed(feedId);
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
        }

        [HttpPost]
        [ActionName("update_feed")]
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.UpdatedFeed feed)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);
            var feedBO = ConvertToBusinessObject(feed);
            userBO.UpdateFeed(feedBO);
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
        }

        #endregion




        #region Article management

        [HttpGet]
        [ActionName("mark_read")]
        public async Task MarkArticleRead(Guid userId, Guid feedId, Guid newsItemId)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);
            await userBO.MarkNewsItemRead(feedId, newsItemId);
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
        }

        [HttpGet]
        [ActionName("mark_unread")]
        public async Task MarkArticleUnread(Guid userId, Guid feedId, Guid newsItemId)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);
            await userBO.MarkNewsItemUnread(feedId, newsItemId);
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
        }

        #endregion




        #region Conversion Helpers

        UserInfo ConvertToBusinessObject(User.DataStore.UserInfo user)
        {
            return user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
        }

        Feed ConvertToBusinessObject(User.DataStore.Feed user)
        {
            return user.Convert<User.DataStore.Feed, Feed>(Converters.Instance);
        }

        UserInfo ConvertToBusinessObject(Incoming.UserInfo user)
        {
            return user.Convert<Incoming.UserInfo, UserInfo>(Converters.Instance);
        }

        Feed ConvertToBusinessObject(Incoming.NewFeed user)
        {
            return user.Convert<Incoming.NewFeed, Feed>(Converters.Instance);
        }

        Feed ConvertToBusinessObject(Incoming.UpdatedFeed user)
        {
            return user.Convert<Incoming.UpdatedFeed, Feed>(Converters.Instance);
        }
        
        
        User.DataStore.UserInfo ConvertToDataStore(UserInfo user)
        {
            return user.Convert<UserInfo, User.DataStore.UserInfo>(Converters.Instance);
        }



        Outgoing.NewsItem ConvertToOutgoing(NewsItem user)
        {
            return user.Convert<NewsItem, Outgoing.NewsItem>(Converters.Instance);
        }

        Outgoing.Feed ConvertToOutgoing(Feed user)
        {
            return user.Convert<Feed, Outgoing.Feed>(Converters.Instance);
        }

        Outgoing.UserInfo ConvertToOutgoing(UserInfo user)
        {
            return user.Convert<UserInfo, Outgoing.UserInfo>(Converters.Instance);
        }

        #endregion
    }
}
