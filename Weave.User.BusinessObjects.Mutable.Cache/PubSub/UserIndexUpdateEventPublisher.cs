using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis.PubSub;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class UserIndexUpdateEventPublisher : RedisPubSubPublisher<UserIndexUpdateNotice>
    {
        public UserIndexUpdateEventPublisher()
            : base(Settings.PubsubConnection, Constants.USER_INDEX_UPDATE_CHANNEL, o => o.WriteToBytes())
        { }
    }
}