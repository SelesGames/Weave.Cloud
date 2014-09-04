using Common.Azure.SmartBlobClient;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class UserInfoBlobClient
    {
        CloudStorageAccount csa;
        string containerName;
        readonly string userAppend = "user";

        public UserInfoBlobClient(CloudStorageAccount csa, string containerName)
        {
            this.csa = csa;
            this.containerName = containerName;
        }

        public async Task<UserInfo> Get(Guid userId)
        {
            var fileName = GetFileName(userId);
            var result = await new SmartBlobClient(csa).Get<UserInfo>(containerName, fileName,
                options: new BlobRequestOptions
                {
                    ServerTimeout = TimeSpan.FromSeconds(15),
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 3),
                    MaximumExecutionTime = TimeSpan.FromSeconds(25),
                });

            return result.Value;
        }

        public Task Save(UserInfo user)
        {
            var fileName = GetFileName(user.Id);
            return new SmartBlobClient(csa).Save(containerName, fileName, user,
                options: new BlobRequestOptions
                {
                    ServerTimeout = TimeSpan.FromSeconds(30),
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 3),
                },
                blobProperties: new BlobProperties
                {
                    ContentType = "application/json",
                    ContentEncoding = "gzip",
                });
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
