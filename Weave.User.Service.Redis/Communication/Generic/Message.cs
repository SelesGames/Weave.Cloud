using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication.Generic
{
    public class Message<T>
    {
        readonly Message innerMessage;
        readonly T value;

        public RedisValue RedisValue { get { return innerMessage.Value; } }
        public T Value { get { return value; } }

        internal Message(Message innerMessage, Func<RedisValue, T> map)
        {
            this.innerMessage = innerMessage;
            this.value = map(innerMessage.Value);
        }

        public Task<bool> Complete()
        {
            return innerMessage.Complete();
        }
    }
}