using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.BusinessObjects.Mutable.Cache;

namespace RedisDBHelper
{
    class GetUserFeedFromCache
    {
        string userId;
        string feedId;

        public GetUserFeedFromCache(string userId, string feedId)
        {
            this.userId = userId;
            this.feedId = feedId;
        }

        public async Task<string> Execute()
        {
            var cache = UserIndexCacheFactory.CreateCache();
            var user = await cache.Get(Guid.Parse(userId));
            var feedGuid = Guid.Parse(feedId);
            var feed = user.FeedIndices.FirstOrDefault(o => o.Id == feedGuid);
            var newsItemCache = ExpandedEntryCacheFactory.CreateCache(0);

            var newsDetails = await newsItemCache.Get(feed.NewsItemIndices.Select(o => o.Id));
            var newsLookup = newsDetails.Results.Where(o => o.HasValue).ToDictionary(o => o.Value.Id, o => o.Value);

            var temp = new
            {
                Id = feed.Id,
                Name = feed.Name,
                Uri = feed.Uri,
                Category = feed.Category,
                News = feed.NewsItemIndices
                    .Select(
                        o =>
                        {
                            ExpandedEntry match;
                            if (newsLookup.TryGetValue(o.Id, out match))
                            {
                                return Flatten(o, match);
                            }
                            else
                            {
                                return Flatten(o);
                            }
                        })
                    .ToList(),
            };

            return temp.Dump();
        }

        object Flatten(NewsItemIndex o)
        {
            return new
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
                HasImage = o.HasImage,
                NoEntry = true,
            };
        }

        object Flatten(NewsItemIndex o, ExpandedEntry e)
        {
            return new
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
                HasImage = o.HasImage,
                Entry = e,
            };
        }
    }
}