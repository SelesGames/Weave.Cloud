using SelesGames.Common;
using System;
using System.Collections.Generic;
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




        #region User management

        [HttpPost]
        [ActionName("create")]
        public async Task<Outgoing.UserInfo> AddUserAndReturnNewNews([FromBody] Incoming.UserInfo incomingUser)
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

        [HttpGet]
        public async Task<Outgoing.UserInfo> GetUserInfoWithNoNews(Guid userId)
        {
            var user = await userRepo.Get(userId);
            foreach (var feed in user.Feeds)
                feed.News = null;

            var userBO = ConvertToBusinessObject(user);
            var outgoing = ConvertToOutgoing(userBO);
            return outgoing;
        }

        [HttpGet]
        public async Task<Outgoing.UserInfo> GetUserInfoWithRefreshedNewsCount(Guid userId)
        {
            var user = await userRepo.Get(userId);
            var userBO = ConvertToBusinessObject(user);
            await userBO.RefreshAllFeeds();
            user = ConvertToDataStore(userBO);
            await userRepo.Save(user);
            
            foreach (var feed in user.Feeds)
                feed.News = null;

            userBO = ConvertToBusinessObject(user);
            var outgoing = ConvertToOutgoing(userBO);
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
        [HttpPost]
        [ActionName("refresh")]
        public async Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId, [FromBody] List<Guid> feedIds)
        {
            TimeSpan readTime, writeTime;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = await userRepo.Get(userId);
            sw.Stop();
            readTime = sw.Elapsed;

            var userBO = ConvertToBusinessObject(user);
            await userBO.RefreshFeedsMatchingIds(feedIds);
            user = ConvertToDataStore(userBO);

            sw = System.Diagnostics.Stopwatch.StartNew();
            await userRepo.Save(user);
            sw.Stop();
            writeTime = sw.Elapsed;

            userBO = ConvertToBusinessObject(user);
            var outgoing = ConvertToOutgoing(userBO);

            outgoing.DataStoreReadTime = readTime;
            outgoing.DataStoreWriteTime = writeTime;
            return outgoing;
        }

        #endregion




        #region Feed management

        [HttpPost]
        [ActionName("add_feed")]
        public async Task AddFeed(Guid userId, [FromBody] Incoming.Feed feed)
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
        public async Task UpdateFeed(Guid userId, [FromBody] Incoming.Feed feed)
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

        Feed ConvertToBusinessObject(Incoming.Feed user)
        {
            return user.Convert<Incoming.Feed, Feed>(Converters.Instance);
        }
        
        
        User.DataStore.UserInfo ConvertToDataStore(UserInfo user)
        {
            return user.Convert<UserInfo, User.DataStore.UserInfo>(Converters.Instance);
        }


        Outgoing.UserInfo ConvertToOutgoing(UserInfo user)
        {
            return user.Convert<UserInfo, Outgoing.UserInfo>(Converters.Instance);
        }

        #endregion
    }
}
