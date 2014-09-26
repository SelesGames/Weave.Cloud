using Common.Azure.Blob;
using Common.Azure.SmartBlobClient;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable.Cache.Azure
{
    public class UserIndexBlobClient
    {
        readonly SmartBlobClient blobClient;
        readonly string containerName;

        public UserIndexBlobClient(string storageAccountName, string storageKey, string containerName)
        {
            this.blobClient = new SmartBlobClient(storageAccountName, storageKey, false);
            this.containerName = containerName;
        }

        public async Task<BlobResult<UserIndex>> Get(Guid userId)
        {
            var blobName = userId.ToString("N");
            var result = await blobClient.Get<Serialization.UserIndex>(containerName, blobName);
            return result.Copy(Map);
        }

        public async Task<bool> Save(UserIndex user)
        {
            var requestProperties = new WriteRequestProperties
            {
                UseCompression = true,
            };
            var blobName = user.Id.ToString("N");
            try
            {
                var mapped = Map(user);
                await blobClient.Save(containerName, blobName, mapped, requestProperties);
                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }




        #region Map functions

        static Serialization.UserIndex Map(UserIndex o)
        {
            return new Serialization.UserIndex
            {
                Id = o.Id,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
                FeedIndices = o.FeedIndices.Select(Map).ToList(),
                LastModified = o.LastModified,
            };
        }

        static Serialization.FeedIndex Map(FeedIndex o)
        {
            return new Serialization.FeedIndex
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                TeaserImageUrl = o.TeaserImageUri,
                ArticleViewingType = o.ArticleViewingType,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
                NewsItemIndices = o.NewsItemIndices.Select(Map).ToList(),
            };
        }

        static Serialization.NewsItemIndex Map(NewsItemIndex o)
        {
            return new Serialization.NewsItemIndex
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
                HasImage = o.HasImage,
            };
        }

        static UserIndex Map(Serialization.UserIndex o)
        {
            var userIndex = new UserIndex
            {
                Id = o.Id,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
                LastModified = o.LastModified,
            };

            if (o.FeedIndices != null)
            {
                foreach (var feedIndex in o.FeedIndices.Select(Map))
                {
                    userIndex.FeedIndices.Add(feedIndex);
                }
            }

            return userIndex;
        }

        static FeedIndex Map(Serialization.FeedIndex o)
        {
            var feedIndex = new FeedIndex
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                TeaserImageUri = o.TeaserImageUrl,
                ArticleViewingType = o.ArticleViewingType,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };

            if (o.NewsItemIndices != null)
            {
                foreach (var newsItemIndex in o.NewsItemIndices.Select(Map))
                {
                    feedIndex.NewsItemIndices.Add(newsItemIndex);
                }
            }

            return feedIndex;
        }

        static NewsItemIndex Map(Serialization.NewsItemIndex o)
        {
            return new NewsItemIndex
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
                HasImage = o.HasImage,
            };
        }

        #endregion
    }
}