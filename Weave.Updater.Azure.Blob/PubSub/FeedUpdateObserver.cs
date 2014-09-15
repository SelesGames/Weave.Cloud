using StackExchange.Redis;
using Weave.User.Service.Redis.PubSub;

namespace Weave.Updater.PubSub
{
    public class FeedUpdateObserver : RedisPubSubObserver<FeedUpdateNotice>
    {
        public FeedUpdateObserver(ConnectionMultiplexer cm)
            : base(cm, Constants.FEED_UPDATE_CHANNEL, o => o.ReadFeedUpdateNotice())
        { }
    }
}