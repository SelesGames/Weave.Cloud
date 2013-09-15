using System;
using Weave.User.DataStore;
using Weave.User.Service.Cache.Extensions;

namespace Weave.User.Service.Cache
{
    public class UserInfoBlobWriteQueue
    {
        UserInfoBlobClient blobClient;

        public UserInfoBlobWriteQueue(UserInfoBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public async void Add(UserInfo user)
        {
            await TaskEx.Retry(() => blobClient.Save(user), 5, TimeSpan.FromSeconds(4));
        }
    }
}
