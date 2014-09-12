using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class RedisPubSubPublisher<T>
    {
        readonly ConnectionMultiplexer cm;
        readonly string channel;
        readonly Func<T, RedisValue> map;

        public RedisPubSubPublisher(ConnectionMultiplexer cm, string channel, Func<T, RedisValue> map)
        {
            if (cm == null) throw new ArgumentNullException("cm");
            if (string.IsNullOrWhiteSpace(channel)) throw new ArgumentException("channel");
            if (map == null) throw new ArgumentNullException("map");

            this.cm = cm;
            this.channel = channel;
            this.map = map;
        }

        public async Task<long> Publish(T o, CommandFlags flags = CommandFlags.None)
        {
            var redisValue = map(o);

            var sub = cm.GetSubscriber();
            var received = await sub.PublishAsync(channel, redisValue, flags);
            return received;
        }
    }
}