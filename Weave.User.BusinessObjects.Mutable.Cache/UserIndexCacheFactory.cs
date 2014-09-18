using StackExchange.Redis;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.Mutable.Cache
{
    public static class UserIndexCacheFactory
    {
        public static async Task<UserIndexCache> CreateCacheAsync()
        {
            var cache = new UserIndexCache(
                azureUserIndexStorageAccountName: "weaveuser2",
                azureUserIndexStorageAccountKey: "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                azureUserIndexContainerName: "userindices",

                legacyUserDataStoreAccountName: "weaveuser",
                legacyUserDataStoreAccountKey: "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==",
                legacyUserDataStoreContainerName: "user");

            await cache.InitializeAsync();
            return cache;
        }
    }
}