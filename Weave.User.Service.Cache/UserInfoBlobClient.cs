using Common.Azure.Blob.Contracts;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class UserInfoBlobClient
    {
        IBlobRepository blobClient;
        string containerName;
        readonly string userAppend = "user";

        public UserInfoBlobClient(IBlobRepository blobClient, string containerName)
        {
            this.blobClient = blobClient;
            this.containerName = containerName;
        }

        public Task<UserInfo> Get(Guid userId)
        {
            var fileName = GetFileName(userId);
            return blobClient.Get<UserInfo>(containerName, fileName);
        }

        public Task Save(UserInfo user)
        {
            var fileName = GetFileName(user.Id);
            return blobClient.Save(containerName, fileName, user);
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
