using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.BusinessObjects.Mutable.Cache;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Clients;

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
            var db = Settings.StandardConnection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var canonicalFeedCache = new FeedUpdaterCache(db);
            var canonicalFeedRep = await canonicalFeedCache.Get(feed.Uri);

            var newsDetails = await newsItemCache.Get(feed.NewsItemIndices.Select(o => o.Id));
            var newsLookup = newsDetails.Results.Where(o => o.HasValue).ToDictionary(o => o.Value.Id, o => o.Value);

            var news = feed.NewsItemIndices
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
                    .ToList();

            var temp = new
            {
                Id = feed.Id,
                Name = feed.Name,
                Uri = feed.Uri,
                Category = feed.Category,
                TotalNewsCount = news.Count(),
                FavoriteCount = news.Count(o => o.IsFavorite),
                ReadCount = news.Count(o => o.HasBeenViewed),
                News = news,
                MissingNews = canonicalFeedRep.Value.News.Where(o => !news.Any(x => x.Id == o.Id)).ToList(),
                ExtraNews = news.Where(o => !canonicalFeedRep.Value.News.Any(x => x.Id == o.Id)).ToList(),
            };

            return temp.Dump();
        }

        dynamic Flatten(NewsItemIndex o)
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