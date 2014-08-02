using StackExchange.Redis;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication
{
    public class MessageQueue
    {
        readonly IDatabase db;
        readonly RedisKey messageList;
        readonly RedisKey processList;

        public long Size { get; private set; }

        public MessageQueue(IDatabase db, RedisKey messageList, RedisKey processList)
        {
            this.db = db;
            this.messageList = messageList;
            this.processList = processList;
        }

        /// <summary>
        /// Push the input value into the message queue
        /// </summary>
        /// <param name="value">The value to push to the message queue</param>
        public async Task Push(RedisValue value)
        {
            Size = await db.ListLeftPushAsync(
                key: messageList,
                value: value,
                when: When.Always,
                flags: CommandFlags.None);
        }

        /// <summary>
        /// Get's the next message from the message queue
        /// </summary>
        /// <returns>A Message object, or null if no messages are in the message queue</returns>
        public async Task<Message> GetNext()
        {
            var redisValue = await db.ListRightPopLeftPushAsync(
                source: messageList,
                destination: processList,
                flags: CommandFlags.None
            );

            if (redisValue.HasValue)
                return new Message(db, processList, redisValue);

            else
                return null;
        }
    }
}
