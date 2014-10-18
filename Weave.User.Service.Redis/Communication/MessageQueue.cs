using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication
{
    public class MessageQueue
    {
        readonly IDatabaseAsync db;
        readonly RedisKey messageList;
        readonly RedisKey processList;

        public long Size { get; private set; }

        public MessageQueue(IDatabaseAsync db, RedisKey messageList, RedisKey processList)
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
        /// <returns>A Message object, or Option.None if no messages are in the message queue</returns>
        public async Task<Option<Message>> GetNext()
        {
            var redisValue = await db.ListRightPopLeftPushAsync(
                source: messageList,
                destination: processList,
                flags: CommandFlags.None
            );

            if (redisValue.HasValue)
                return Option.Some(new Message(db, processList, redisValue));

            else
                return Option.None<Message>();
        }
    }
}