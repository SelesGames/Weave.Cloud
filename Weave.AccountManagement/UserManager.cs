using Common.Azure;
using Common.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.AccountManagement.DTOs
{
    public class UserManager
    {
        AzureBlobClient<UserInfo> client;
        ValidationEngine validationEngine;

        public UserManager(AzureBlobClient<UserInfo> client)
        {
            this.client = client;
            this.validationEngine = new UserInfoValidator();
        }




        #region simple CRUD operations against the UserInfo

        /// <summary>
        /// Retrieves a User from blob storage
        /// </summary>
        public Task<UserInfo> Get(Guid userId)
        {
            return client.Get(userId);
        }

        /// <summary>
        /// Adds or updates a User to blob storage
        /// </summary>
        public Task Save(UserInfo user)
        {
            if (validationEngine != null)
                validationEngine.Validate(user);

            var id = user.Id;

            user.FeedCount = user.Feeds.Count;
            return client.Save(id, user);
        }

        /// <summary>
        /// Delets a user from blob storage
        /// </summary>
        public Task Delete(Guid userId)
        {
            return client.Delete(userId);
        }

        #endregion




        public async Task AddOrUpdateFeeds(Guid userId, IEnumerable<Feed> feeds)
        {
            var userInfo = await client.Get(userId);

            if (userInfo.Feeds == null)
                userInfo.Feeds = new List<Feed>();

            var usersFeeds = userInfo.Feeds;

            foreach (var feed in feeds)
            {
                if (validationEngine != null)
                    validationEngine.Validate(feed);

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
