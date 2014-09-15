using StackExchange.Redis;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis.PubSub;

namespace Weave.Updater.PubSub
{
    public class FeedUpdatePublisher : RedisPubSubPublisher<FeedUpdateNotice>
    {
        public FeedUpdatePublisher(ConnectionMultiplexer cm)
            : base(cm, Constants.FEED_UPDATE_CHANNEL, o => o.WriteToBytes())
        { }

        public Task<long> Publish(FeedUpdate update)
        {
            var notice = new FeedUpdateNotice { FeedUri = update.Feed.Uri, RefreshTime = update.RefreshTime };
            return Publish(notice);
        }
    }
}