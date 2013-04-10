//using Common.Azure;
//using Common.Azure.SmartBlobClient;
//using Microsoft.WindowsAzure.StorageClient;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Weave.User.DataStore;
//using System.Linq;

//namespace Weave.UserFeedAggregator.Repositories
//{
//    public class UserRepository
//    {
//        AzureCredentials cred;
//        AzureBlobClient accountHelper;
//        readonly string userAppend = "user";

//        public UserRepository(AzureCredentials cred)
//        {
//            this.cred = cred;
//            accountHelper = new AzureBlobClient(cred.AccountName, cred.AccountKey, cred.UseHttps);
//        }

//        public async Task<UserInfo> Get(Guid userId)
//        {
//            var client = CreateClient(userId);
//            var userInfo = await client.Get<UserInfo>(userAppend);
//            if (userInfo.Feeds != null)
//            {
//                var feeds = await Task.WhenAll(userInfo.Feeds.Select(o => client.Get<FeedNews>(o.Id)));
//                var feedsLookup = feeds.ToDictionary(o => o.FeedId);
//                foreach (var feed in userInfo.Feeds)
//                {
//                    var key = feed.Id;
//                    if (feedsLookup.ContainsKey(key))
//                        feed.News = feedsLookup[key].News;
//                }
//            }
//            return userInfo;
//        }

//        public async Task Add(UserInfo user)
//        {
//            await accountHelper.CreateContainer(user.Id.ToString());
//            await Save(user);
//        }

//        public async Task Save(UserInfo user)
//        {
//            var client = CreateClient(user.Id);
//            if (user.Feeds != null)
//            {
//                await Task.WhenAll(user.Feeds.Select(o => client.Save(o.Id, new FeedNews { FeedId = o.Id, News = o.News })));
//                foreach (var feed in user.Feeds)
//                    feed.News = null;
//            }
//            await client.Save(userAppend, user);
//        }

//        public Task Delete(Guid userId)
//        {
//            return accountHelper.DeleteContainer(userId.ToString());
//        }

//        public Task Delete(UserInfo user)
//        {
//            return Delete(user.Id);
//        }

//        IAzureBlobClient CreateClient(Guid userId)
//        {
//            var client = new SmartBlobClient(cred.AccountName, cred.AccountKey, userId.ToString(), cred.UseHttps)
//            {
//                ContentType = "application/json"
//            };
//            return client;
//        }
//    }
//}





using Common.Azure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.User.DataStore;

namespace Weave.UserFeedAggregator.Repositories
{
    public class UserRepository
    {
        IAzureBlobClient blobClient;
        readonly string userAppend = "user";

        public UserRepository(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public Task<UserInfo> Get(Guid userId)
        {
            var fileName = GetFileName(userId);
            return blobClient.Get<UserInfo>(fileName);
        }

        public async Task<UserInfo> GetOrCreate(Guid userId)
        {
            UserInfo user = null;
            try
            {
                user = await Get(userId);
            }
            catch (StorageClientException storageException)
            {
                System.Diagnostics.Debug.WriteLine(storageException);
                user = new UserInfo
                {
                    Id = userId,
                    FeedCount = 0,
                    Feeds = new List<Feed>(),
                };
            }
            return user;
        }

        public Task Save(UserInfo user)
        {
            var fileName = GetFileName(user.Id);
            return blobClient.Save(fileName, user);
        }

        public Task Delete(Guid userId)
        {
            var fileName = GetFileName(userId);
            return blobClient.Delete(fileName);
        }

        public Task Delete(UserInfo user)
        {
            return Delete(user.Id);
        }




        #region helper methods

        string GetFileName(Guid userId)
        {
            var fileName = string.Format("{0}.{1}", userId, userAppend);
            return fileName;
        }

        #endregion
    }
}
