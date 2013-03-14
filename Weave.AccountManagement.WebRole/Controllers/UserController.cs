using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.AccountManagement.DTOs;

namespace Weave.AccountManagement.WebRole.Controllers
{
    public class UserController : ApiController
    {
        UserManager manager;

        public UserController(UserManager manager)
        {
            this.manager = manager;
        }

        public Task<UserInfo> Get(Guid userId)
        {
            return manager.Get(userId);
        }

        public Task Save(UserInfo user)
        {
            return manager.Save(user);
        }

        public Task Delete(Guid userId)
        {
            return manager.Delete(userId);
        }

        public Task SaveFeed(Guid userId, List<Feed> feeds)
        {
            return manager.SaveFeed(userId, feeds);
        }

        public Task RemoveFeeds(Guid userId, List<Guid> feedIds)
        {
            return manager.RemoveFeeds(userId, feedIds);
        }
    }
}