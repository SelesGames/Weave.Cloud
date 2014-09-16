using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis.PubSub;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class UserIndexUpdateEventObserver : RedisPubSubObserver<UserIndexUpdateNotice>
    {
        public UserIndexUpdateEventObserver()
            : base(Settings.PubsubConnection, Constants.USER_INDEX_UPDATE_CHANNEL, o => o.ReadUserIndexUpdateNotice())
        { }
    }
}