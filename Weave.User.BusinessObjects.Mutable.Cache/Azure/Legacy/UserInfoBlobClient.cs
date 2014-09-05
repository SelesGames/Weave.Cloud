using Common.Azure.SmartBlobClient;
using Common.TimeFormatting;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;

namespace Weave.User.BusinessObjects.Mutable.Cache.Azure.Legacy
{
    public class UserInfoBlobClient
    {
        readonly string containerName;
        readonly string userAppend = "user";
        readonly SmartBlobClient client;

        public UserInfoBlobClient(string accountName, string accountKey, string containerName)
        {
            this.containerName = containerName;
            this.client = new SmartBlobClient(accountName, accountKey, false);
        }

        public async Task<UserIndex> Get(Guid userId)
        {
            var fileName = GetFileName(userId);
            var result = await client.Get<UserInfo>(containerName, fileName);
            if (result.HasValue)
            {
                return Map(result.Value);
            }
            return null;
        }




        #region Map functions

        static UserIndex Map(UserInfo o)
        {
            var index = new UserIndex
            {
                Id = o.Id,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
                CurrentLoginTime = o.CurrentLoginTime,
                PreviousLoginTime = o.PreviousLoginTime,
            };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds)
                {
                    var feedIndex = Map(feed);
                    index.FeedIndices.Add(feedIndex);
                }
            }

            return index;
        }

        static FeedIndex Map(Feed o)
        {
            var index = new FeedIndex
            {
                Id = o.Id,
                Uri = o.FeedUri,
                Name = o.FeedName,
                IconUri = o.IconUri,
                Category = o.Category,
                TeaserImageUrl = null,
                ArticleViewingType = o.ArticleViewingType,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };

            if (o.News != null)
            {
                foreach (var newsItem in o.News)
                {
                    var newsItemIndex = Map(newsItem);
                    index.NewsItemIndices.Add(newsItemIndex);
                }
            }

            return index;
        }

        static NewsItemIndex Map(NewsItem o)
        {
            return new NewsItemIndex
            {
                Id = o.Id,
                UtcPublishDateTime = Parse(o.UtcPublishDateTime),
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
                HasImage = o.Image != null || !string.IsNullOrEmpty(o.ImageUrl),
            };
        }

        static DateTime Parse(string s)
        {
            var attempt = s.TryGetUtcDate();
            if (attempt.Item1)
                // we have to call ".ToUniversalTime()" because .NET automatically converts the string to be in local time
                return attempt.Item2.ToUniversalTime();
            else
                return DateTime.MinValue;
        }

        #endregion




        #region helper methods

        string GetFileName(Guid userId)
        {
            var fileName = string.Format("{0}.{1}", userId, userAppend);
            return fileName;
        }

        #endregion
    }
}
