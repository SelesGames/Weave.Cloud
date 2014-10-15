using StackExchange.Redis;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Communication.Generic;

namespace Weave.FeedUpdater.Messaging
{
    public class FeedUpdateMessageQueue : MessageQueue<FeedUpdateNotice>
    {
        public FeedUpdateMessageQueue()
            : base(CreateDatabase(), "feedUpdateMQ", "feedUpdatePL", new FeedUpdateNoticeMap())
        { }

        static IDatabaseAsync CreateDatabase()
        {
            return Settings.StandardConnection.GetDatabase(DatabaseNumbers.MESSAGE_QUEUE);
        }

        public Task Push(FeedUpdate update)
        {
            var notice = new FeedUpdateNotice { FeedUri = update.Feed.Uri, RefreshTime = update.RefreshTime };
            return Push(notice);
        }
    }
}