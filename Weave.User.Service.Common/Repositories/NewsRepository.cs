using Common.Azure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.User.DataStore;

namespace Weave.UserFeedAggregator.Repositories
{
    public class NewsRepository
    {
        IAzureBlobClient blobClient;
        readonly string newsAppend = "articles";

        public NewsRepository(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public Task<UserFeeds> Get(Guid userId)
        {
            var fileName = GetFileName(userId);
            return blobClient.Get<UserFeeds>(fileName);
        }

        public async Task<UserFeeds> GetOrCreate(Guid userId)
        {
            UserFeeds user = null;
            try
            {
                user = await Get(userId);
            }
            catch (StorageClientException storageException)
            {
                System.Diagnostics.Debug.WriteLine(storageException);
                user = new UserFeeds
                {
                    UserId = userId,
                    FeedsNews = new List<FeedNews>(),
                };
            }
            return user;
        }

        public Task Save(UserFeeds user)
        {
            var fileName = GetFileName(user.UserId);
            return blobClient.Save(fileName, user);
        }

        public Task Delete(Guid userId)
        {
            var fileName = GetFileName(userId);
            return blobClient.Delete(fileName);
        }

        public Task Delete(UserFeeds user)
        {
            return Delete(user.UserId);
        }




        #region helper methods

        string GetFileName(Guid userId)
        {
            var fileName = string.Format("{0}.{1}", userId, newsAppend);
            return fileName;
        }

        #endregion
    }
}
