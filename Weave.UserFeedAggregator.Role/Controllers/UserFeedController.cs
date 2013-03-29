using SelesGames.Common;
using System;
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

        //public async Task Get(Guid userId)
        //{
        //    var user = await userClient.Get<UserInfo>(userId.ToString());
        //}

        [HttpPost]
        [ActionName("createUser")]
        public async Task<User.DataStore.UserInfo> AddUserAndReturnNews([FromBody] User.DataStore.UserInfo user)
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
    }
}
