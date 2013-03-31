﻿using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.UserFeedAggregator.BusinessObjects;
using Weave.UserFeedAggregator.Repositories;

namespace Weave.UserFeedAggregator.Role.Controllers
{
    public class UserFeedController : ApiController
    {
        UserRepository userRepo;

        public UserFeedController(UserRepository userRepo)
        {
            this.userRepo = userRepo;
        }

        public async Task<User.DataStore.UserInfo> GetUserInfoWithNoNews(Guid userId)
        {
            var user = await userRepo.Get(userId);
            foreach (var feed in user.Feeds)
                feed.News = null;

            return user;
        }

        public async Task<User.DataStore.UserInfo> GetUserInfoWithRefreshedNewsCount(Guid userId)
        {
            var user = await userRepo.Get(userId);
            var userBO = user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
            await userBO.RefreshAllFeeds();
            user = userBO.Convert<UserInfo, User.DataStore.UserInfo>(Converters.Instance);
            await userRepo.Save(user);
            
            foreach (var feed in user.Feeds)
                feed.News = null;

            return user;
        }

        [HttpPost]
        [ActionName("createUser")]
        public async Task<User.DataStore.UserInfo> AddUserAndReturnNewNews([FromBody] User.DataStore.UserInfo user)
        {
            await userRepo.Save(user);
            var userBO = user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
            await userBO.RefreshAllFeeds();
            user = userBO.Convert<UserInfo, User.DataStore.UserInfo>(Converters.Instance);
            await userRepo.Save(user);
            return user;
        }

        public async Task<User.DataStore.UserInfo> RefreshAndReturnNews(Guid userId)
        {
            var user = await userRepo.Get(userId);
            var userBO = user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
            await userBO.RefreshAllFeeds();
            user = userBO.Convert<UserInfo, User.DataStore.UserInfo>(Converters.Instance);
            await userRepo.Save(user);
            return user;
        }

        public async Task<User.DataStore.UserInfo> RefreshAndReturnNews(Guid userId, List<Guid> feedIds)
        {
            var user = await userRepo.Get(userId);
            var userBO = user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
            await userBO.RefreshFeedsMatchingIds(feedIds);
            user = userBO.Convert<UserInfo, User.DataStore.UserInfo>(Converters.Instance);
            await userRepo.Save(user);
            return user;
        }
        
        public async Task MarkArticleRead(Guid userId, Guid feedId, Guid newsItemId)
        {
            var user = await userRepo.Get(userId);
            var userBO = user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
            userBO.MarkNewsItemRead(feedId, newsItemId);
        }

        public async Task MarkArticleUnread(Guid userId, Guid feedId, Guid newsItemId)
        {
            var user = await userRepo.Get(userId);
            var userBO = user.Convert<User.DataStore.UserInfo, UserInfo>(Converters.Instance);
            userBO.MarkNewsItemUnread(feedId, newsItemId);
        }
    }
}
