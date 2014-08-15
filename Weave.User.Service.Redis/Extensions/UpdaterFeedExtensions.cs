using StackExchange.Redis;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis
{
    public static class UpdaterFeedExtensions
    {
        /// <summary>
        /// Recover the feed's state from Redis.  this will be used whenever the service restarts
        /// </summary>
        public static async Task RecoverStateFromRedis(this Feed feed, IDatabaseAsync db)
        {
            var cache = new FeedUpdaterCache(db);

            var cachedDataResult = await cache.Get(feed.Id);
            if (cachedDataResult.HasValue)
            {
                var cachedData = cachedDataResult.Value;
                CopyState(source: cachedData, target: feed);
            }
        }

        static void CopyState(Feed source, Feed target)
        {
            target.TeaserImageUrl = source.TeaserImageUrl;
            target.LastRefreshedOn = source.LastRefreshedOn;
            target.Etag = source.Etag;
            target.LastModified = source.LastModified;
            target.MostRecentNewsItemPubDate = source.MostRecentNewsItemPubDate;

            foreach (var entry in source.Entries)
                target.Entries.Add(entry);
        }
    }
}