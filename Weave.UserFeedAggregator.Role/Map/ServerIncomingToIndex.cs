using System.Linq;
using Weave.User.BusinessObjects;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;

namespace Weave.User.Service.Role.Map
{
    public static class ServerIncomingToBusinessObject
    {
        public static Feed Convert(Incoming.NewFeed o)
        {
            return new Feed
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public static UserInfo Convert(Incoming.UserInfo o)
        {
            var user = new UserInfo { Id = o.Id, };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<Incoming.NewFeed>().Select(Convert))
                    user.Feeds.Add(feed);
            }

            return user;
        }
    }
}