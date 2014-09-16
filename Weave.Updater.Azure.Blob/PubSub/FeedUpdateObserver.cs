using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis.PubSub;

namespace Weave.Updater.PubSub
{
    public class FeedUpdateObserver : RedisPubSubObserver<FeedUpdateNotice>
    {
        public FeedUpdateObserver()
            : base(Settings.PubsubConnection, Constants.FEED_UPDATE_CHANNEL, o => o.ReadFeedUpdateNotice())
        { }
    }
}