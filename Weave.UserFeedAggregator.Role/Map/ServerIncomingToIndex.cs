using System.Linq;
using Weave.User.BusinessObjects.Mutable;
using Incoming = Weave.Services.User.DTOs.ServerIncoming;

namespace Weave.User.Service.Role.Map
{
    public static class ServerIncomingToBusinessObject
    {
        public static UserIndex Convert(Incoming.UserInfo o)
        {
            var user = new UserIndex { Id = o.Id, };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<Incoming.NewFeed>().Select(Convert))
                    user.FeedIndices.Add(feed);
            }

            return user;
        }

        static FeedIndex Convert(Incoming.NewFeed o)
        {
            return new FeedIndex
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }
    }
}