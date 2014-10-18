using StackExchange.Redis;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Communication.Generic;

namespace Weave.User.BusinessObjects.Mutable.Cache.Messaging
{
    public class UserIndexUpdateMessageQueue : MessageQueue<UserIndexUpdateNotice>
    {
        public UserIndexUpdateMessageQueue()
            : base(CreateDatabase(), "userIndexUpdateMQ", "userIndexUpdatePL", new UserIndexUpdateNoticeMap())
        { }

        static IDatabaseAsync CreateDatabase()
        {
            return Settings.StandardConnection.GetDatabase(DatabaseNumbers.MESSAGE_QUEUE);
        }
    }
}