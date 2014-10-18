using System;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.BusinessObjects.Mutable.Cache.Azure;
using Weave.User.BusinessObjects.Mutable.Cache.Messaging;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Clients;
using Weave.User.Service.Redis.Communication.Generic;

namespace Weave.User.Service.UpdateProcessor
{
    public class UserIndexPersister : RedisPersister<UserIndexUpdateNotice, UserIndex>
    {
        public UserIndexPersister()
            : base(new UserIndexUpdateMessageQueue(), CreateGet(), CreatePersist(), TimeSpan.FromMilliseconds(30))
        { }

        static Func<UserIndexUpdateNotice, Task<RedisCacheResult<UserIndex>>> CreateGet()
        {
            var connection = Settings.StandardConnection;
            var redisCache = new UserIndexCache(connection);
            return update => redisCache.Get(update.UserId);
        }

        static Func<UserIndex, Task<bool>> CreatePersist()
        {
            var blobClient = new UserIndexBlobClient(
                storageAccountName: "weaveuser2",
                storageKey: "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                containerName: "userindices"); 
            
            return val => blobClient.Save(val);
        }
    }
}