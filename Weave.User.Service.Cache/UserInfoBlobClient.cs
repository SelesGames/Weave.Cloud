using Common.Azure;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class UserInfoBlobClient
    {
        IAzureBlobClient blobClient;
        readonly string userAppend = "user";

        public UserInfoBlobClient(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public Task<UserInfo> Get(Guid userId)
        {
            var fileName = GetFileName(userId);
            return blobClient.Get<UserInfo>(fileName);
        }

        public Task Save(UserInfo user)
        {
            var fileName = GetFileName(user.Id);
            return blobClient.Save(fileName, user);
        }

        //public Task Delete(Guid userId)
        //{
        //    var fileName = GetFileName(userId);
        //    return blobClient.Delete(fileName);
        //}

        //public Task Delete(UserInfo user)
        //{
        //    return Delete(user.Id);
        //}




        #region helper methods

        string GetFileName(Guid userId)
        {
            var fileName = string.Format("{0}.{1}", userId, userAppend);
            return fileName;
        }

        #endregion
    }
}
