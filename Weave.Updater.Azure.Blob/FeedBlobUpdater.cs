using System.Threading.Tasks;
using Redis = Weave.User.Service.Redis;

namespace Weave.Updater.Azure.Blob
{
    public class FeedBlobUpdater
    {
        FeedUpdaterCache blobCache; 
        Redis.FeedUpdaterCache redisCache;

        public FeedBlobUpdater(FeedUpdaterCache blobCache, Redis.FeedUpdaterCache redisCache)
        {
            this.blobCache = blobCache;
            this.redisCache = redisCache;
        }

        /// <summary>
        /// Grabs the latest values for an updater feed from Redis, and updates Blob storage
        /// </summary>
        /// <param name="feedUrl">The feed's url, which is used as the key/blobName</param>
        /// <returns>True if the value was updated - otherwise false</returns>
        public async Task<bool> Update(string feedUrl)
        {
            var redisResult = await redisCache.Get(feedUrl);
            if (redisResult.HasValue)
            {
                var latest = redisResult.Value;
                return await blobCache.Save(latest);
            }

            return false;
        }
    }
}