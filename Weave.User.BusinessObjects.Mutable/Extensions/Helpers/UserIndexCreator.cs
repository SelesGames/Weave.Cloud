using SelesGames.Common.Hashing;
using System;

namespace Weave.User.BusinessObjects.Mutable.Extensions.Helpers
{
    static class UserIndexCreator
    {
        public static UserIndex Create(UserInfo u)
        {
            var userIndex = CreateUserIndex(u);

            if (u.Feeds != null)
            {
                foreach (var f in u.Feeds)
                {
                    var feedIndex = CreateFeedIndex(f);

                    if (f.News != null)
                    {
                        foreach (var n in f.News)
                        {
                            var newsItemIndex = CreateNewsItemIndex(n);
                            feedIndex.NewsItemIndices.Add(newsItemIndex);
                        }
                    }

                    userIndex.FeedIndices.Add(feedIndex);
                }
            }

            return userIndex;
        }

        static UserIndex CreateUserIndex(UserInfo o)
        {
            return new UserIndex
            {
                Id = o.Id,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
            };
        }

        static FeedIndex CreateFeedIndex(Feed o)
        {
            return new FeedIndex
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                TeaserImageUrl = o.TeaserImageUrl,
                ArticleViewingType = o.ArticleViewingType,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };
        }

        static NewsItemIndex CreateNewsItemIndex(NewsItem o)
        {
            return new NewsItemIndex
            {
                Id = o.Id,
                //UrlHash = ComputeHash(o.Link),
                //TitleHash = ComputeHash(o.Title),
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = o.IsFavorite,
                HasBeenViewed = o.HasBeenViewed,
                HasImage = o.HasImage,
            };
        }

        //static long ComputeHash(string val)
        //{
        //    var guid = CryptoHelper.ComputeHashUsedByMobilizer(val);
        //    var byteArray = guid.ToByteArray();
        //    var result = BitConverter.ToInt64(byteArray, 0);
        //    return result;
        //}
    }
}