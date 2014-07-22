using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Service.Cache.Map;

namespace Weave.User.Service.Cache
{
    public class UserRepository
    {
        UserInfoBlobWriteQueue writeQueue;
        UserInfoBlobClient userInfoBlobClient;

        public UserRepository(UserInfoBlobClient userInfoBlobClient)
        {
            this.userInfoBlobClient = userInfoBlobClient;
            writeQueue = new UserInfoBlobWriteQueue(userInfoBlobClient);
        }

        public async Task<UserInfo> Get(Guid userId)
        {
            var store = await userInfoBlobClient.Get(userId);
            var user = DataStoreToBusinessObject.Convert(store);
            return user;
        }

        public void Save(Guid userId, UserInfo user)
        {
            var store = BusinessObjectToDataStore.Convert(user);
            writeQueue.Add(store);
        }
    }
}
