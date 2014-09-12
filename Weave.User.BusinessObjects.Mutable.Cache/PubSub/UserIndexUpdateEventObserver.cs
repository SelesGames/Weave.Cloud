using StackExchange.Redis;
using Weave.User.Service.Redis.PubSub;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class UserIndexUpdateEventObserver : RedisPubSubObserver<UserIndexUpdateNotice>
    {
        public UserIndexUpdateEventObserver(ConnectionMultiplexer cm)
            : base(cm, Constants.USER_INDEX_UPDATE_CHANNEL, o => o.ReadUserIndexUpdateNotice())
        { }
    }
}