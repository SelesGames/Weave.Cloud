using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.User.Service.Redis.Serializers;

namespace Weave.User.Service.Redis.Communication.Generic
{
    public class MessageQueue<T>
    {
        readonly MessageQueue innerQueue;
        readonly IRedisValueMap<T> map;

        public long Size { get { return innerQueue.Size; } }

        public MessageQueue(IDatabaseAsync db, RedisKey messageList, RedisKey processList, IRedisValueMap<T> map)
        {
            this.innerQueue = new MessageQueue(db, messageList, processList);
            this.map = map;
        }

        /// <summary>
        /// Push the input value into the message queue
        /// </summary>
        /// <param name="value">The value to push to the message queue</param>
        public Task Push(T o)
        {
            RedisValue value;
            try
            {
                value = map.Map(o);
            }
            catch(Exception ex)
            {
                throw new SerializationException(ex, RedisValue.Null);
            }
            return innerQueue.Push(value);
        }

        /// <summary>
        /// Get's the next message from the message queue
        /// </summary>
        /// <returns>A Message object, or Option.None if no messages are in the message queue</returns>
        public async Task<Option<Message<T>>> GetNext()
        {
            var next = await innerQueue.GetNext();
            if (next.IsSome)
            {     
                var message = next.Value;

                try
                {
                    var mappedVal = map.Map(message.Value);
                    return Option.Some(new Message<T>(message, mappedVal));
                }
                catch(Exception e)
                {
                    throw new SerializationException(e, message.Value);
                }
            }

            else
                return Option.None<Message<T>>();
        }
    }
}