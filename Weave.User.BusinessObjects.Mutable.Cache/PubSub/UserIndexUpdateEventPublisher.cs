using StackExchange.Redis;
using Weave.User.Service.Redis.PubSub;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class UserIndexUpdateEventPublisher : RedisPubSubPublisher<UserIndexUpdateNotice>
    {
        public UserIndexUpdateEventPublisher(ConnectionMultiplexer cm)
            : base(cm, Constants.USER_INDEX_UPDATE_CHANNEL, o => o.WriteToBytes())
        { }
    }
}