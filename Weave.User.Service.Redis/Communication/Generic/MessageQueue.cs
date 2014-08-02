using StackExchange.Redis;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication.Generic
{
    public abstract class MessageQueue<T>
    {
        readonly MessageQueue innerQueue;

        public long Size { get { return innerQueue.Size; } }

        public MessageQueue(IDatabase db, RedisKey messageList, RedisKey processList)
        {
            this.innerQueue = new MessageQueue(db, messageList, processList);
        }

        protected abstract T Map(RedisValue value);
        protected abstract RedisValue Map(T o);

        /// <summary>
        /// Push the input value into the message queue
        /// </summary>
        /// <param name="value">The value to push to the message queue</param>
        public Task Push(T o)
        {
            var value = Map(o);
            return innerQueue.Push(value);
        }

        /// <summary>
        /// Get's the next message from the message queue
        /// </summary>
        /// <returns>A Message object, or null if no messages are in the message queue</returns>
        public async Task<Message<T>> GetNext()
        {
            var message = await innerQueue.GetNext();
            return message == null ? null : new Message<T>(message, Map);
        }
    }
}