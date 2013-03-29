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
