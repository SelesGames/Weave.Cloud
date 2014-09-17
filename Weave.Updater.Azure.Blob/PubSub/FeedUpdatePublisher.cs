using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis.PubSub;

namespace Weave.FeedUpdater.PubSub
{
    public class FeedUpdatePublisher : RedisPubSubPublisher<FeedUpdateNotice>
    {
        public FeedUpdatePublisher()
            : base(Settings.PubsubConnection, Constants.FEED_UPDATE_CHANNEL, o => o.WriteToBytes())
        { }

        public Task<long> Publish(FeedUpdate update)
        {
            var notice = new FeedUpdateNotice { FeedUri = update.Feed.Uri, RefreshTime = update.RefreshTime };
            return Publish(notice);
        }
    }
}