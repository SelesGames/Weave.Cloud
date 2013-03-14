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

        public Task<UserInfo> Get(Guid id)
        {
            return manager.Get(id);
        }

        public Task Post([FromBody] UserInfo user)
        {
            return manager.Save(user);
        }

        public Task Delete(Guid userId)
        {
            return manager.Delete(userId);
        }

        [HttpPost]
        [ActionName("SaveFeeds")]
        public Task SaveFeeds(Guid userId, [FromBody] List<Feed> feeds)
        {
            return manager.SaveFeed(userId, feeds);
        }

        [HttpPost]
        [ActionName("RemoveFeeds")]
        public Task RemoveFeeds(Guid userId, [FromBody] List<Guid> feedIds)
        {
            return manager.RemoveFeeds(userId, feedIds);
        }

        [HttpPost]
        [ActionName("SaveFeed")]
        public Task SaveFeed(Guid userId, [FromBody] Feed feed)
        {
            return manager.SaveFeed(userId, new[] { feed });
        }

        [HttpPost]
        [ActionName("RemoveFeed")]
        public Task RemoveFeed(Guid userId, Guid feedId)
        {
            return manager.RemoveFeeds(userId, new[] { feedId });
        }
    }
}