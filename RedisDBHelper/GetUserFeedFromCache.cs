﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.BusinessObjects.Mutable.Cache;
using Weave.User.Service.Redis;

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
            var conn = Settings.StandardConnection;
            var db = conn.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var newsItemCache = new Weave.User.Service.Redis.Clients.ExpandedEntryCache(db);
            var newsDetails = await newsItemCache.Get(feed.NewsItemIndices.Select(o => o.Id));
            var newsLookup = newsDetails.GetValidValues().ToDictionary(o => o.Id);

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