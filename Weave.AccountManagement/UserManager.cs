using Common.Azure;
using Common.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.AccountManagement
{
    public class UserManager : IAzureBlobClient<UserInfo>
    {
        AzureBlobClient<UserInfo> client;
        ValidationEngine validationEngine;

        public UserManager(AzureBlobClient<UserInfo> client)
        {
            this.client = client;
            this.validationEngine = new UserInfoValidator();
        }




        #region simple CRUD operations against the UserInfo

        public Task<UserInfo> Get(Guid userId)
        {
            return client.Get(userId);
        }

        public Task Save(UserInfo obj)
        {
            var id = obj.Id;
            return client.Save(id, obj);
        }

        public Task Delete(Guid userId)
        {
            return client.Delete(userId);
        }

        #endregion




        public async Task SaveFeed(Guid userId, IEnumerable<Feed> feeds)
        {
            var userInfo = await client.Get(userId);

            if (userInfo.Feeds == null)
                userInfo.Feeds = new List<Feed>();

            var usersFeeds = userInfo.Feeds;

            foreach (var feed in feeds)
            {
                var existing = usersFeeds.SingleOrDefault(o => o.Id.Equals(feed.Id));

                if (existing != null)
                    usersFeeds.Remove(existing);

                usersFeeds.Add(feed);
            }

            await client.Save(userId, userInfo);
        }

        public async Task RemoveFeeds(Guid userId, IEnumerable<Guid> feedIds)
        {
            var userInfo = await client.Get(userId);

            var usersFeeds = userInfo.Feeds;

            foreach (var feedId in feedIds)
            {
                var existing = usersFeeds.SingleOrDefault(o => o.Id.Equals(feedId));

                if (existing != null)
                    usersFeeds.Remove(existing);
            }

            await client.Save(userId, userInfo);
        }
    }
}
