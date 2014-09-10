using System.Linq;
using Weave.User.BusinessObjects.Mutable;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;


namespace Weave.User.Service.Role.Map
{
    public class BusinessObjectToServerOutgoing
    {
        public static Outgoing.Feed Convert(FeedIndex o)
        {
            return new Outgoing.Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                ArticleViewingType = (Weave.User.Service.DTOs.ArticleViewingType)o.ArticleViewingType,
                TotalArticleCount = o.NewsItemIndices.Count,
                NewArticleCount = o.NewsItemIndices.CountNew(),
                UnreadArticleCount = o.NewsItemIndices.CountUnread(),
                TeaserImageUrl = o.TeaserImageUrl,
                //LastRefreshedOn = o.LastRefreshedOn,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };
        }

        public static Outgoing.UserInfo Convert(UserIndex o)
        {
            return new Outgoing.UserInfo
            {
                Id = o.Id,
                FeedCount = o.FeedIndices.Count,
                Feeds = o.FeedIndices.Select(Convert).ToList(),
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
                LastModified = o.LastModified,              
            };
        }
    }
}