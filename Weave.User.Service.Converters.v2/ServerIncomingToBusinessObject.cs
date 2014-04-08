using SelesGames.Common;
using System.Linq;
using Weave.User.BusinessObjects.v2;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;

namespace Weave.User.Service.Converters.v2
{
    public class ServerIncomingToBusinessObject :
        IConverter<Incoming.NewFeed, Feed>,
        IConverter<Incoming.UpdatedFeed, Feed>,
        IConverter<Incoming.UserInfo, UserInfo>
    {
        public static readonly ServerIncomingToBusinessObject Instance = new ServerIncomingToBusinessObject();

        public Feed Convert(Incoming.NewFeed o)
        {
            return new Feed
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public Feed Convert(Incoming.UpdatedFeed o)
        {
            return new Feed
            {
                Id = o.Id,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public UserInfo Convert(Incoming.UserInfo o)
        {
            var user = new UserInfo { Id = o.Id, };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<Incoming.NewFeed>().Select(x => x.Convert<Incoming.NewFeed, Feed>(Instance)))
                    user.Feeds.TryAdd(feed, trustSource: false);
            }

            return user;
        }
    }
}
