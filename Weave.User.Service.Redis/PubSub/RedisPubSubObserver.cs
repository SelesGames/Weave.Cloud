using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.User.Service.Redis.Serializers;

namespace Weave.User.Service.Redis.PubSub
{
    public class RedisPubSubObserver<T>
    {
        readonly ConnectionMultiplexer cm;
        readonly string channel;
        readonly Func<RedisValue, T> map;

        public RedisPubSubObserver(ConnectionMultiplexer cm, string channel, Func<RedisValue, T> map)
        {
            if (cm == null) throw new ArgumentNullException("cm");
            if (string.IsNullOrWhiteSpace(channel)) throw new ArgumentException("channel");
            if (map == null) throw new ArgumentNullException("map");

            this.cm = cm;
            this.channel = channel;
            this.map = map;
        }

        public async Task<IDisposable> Observe(Action<T> onNoticeReceived, Action<SerializationException> onParseException = null, CommandFlags flags = CommandFlags.None)
        {
            Action<RedisChannel, RedisValue> handler = (c, v) => 
                OnNoticeReceived(c, v, onNoticeReceived, onParseException);

            var sub = cm.GetSubscriber();
            await sub.SubscribeAsync(
                channel: channel,
                handler: handler,
                flags: flags);

            return new DisposeHelper(async () => { try { await sub.UnsubscribeAsync(channel, handler); } catch { } });
        }

        void OnNoticeReceived(RedisChannel channel, RedisValue redisValue, Action<T> onNoticeReceived, Action<SerializationException> onParseException)
        {
            if (onParseException != null)
            {
                try
                {
                    var value = map(redisValue);
                    if (value != null)
                    {
                        onNoticeReceived(value);
                    }
                }
                catch(Exception ex)
                {
                    var parseException = new SerializationException(ex, redisValue);
                    onParseException(parseException);
                }
            }
            else
            {
                try
                {
                    var value = map(redisValue);
                    if (value != null)
                    {
                        onNoticeReceived(value);
                    }
                }
                catch { }
            }
        }

        class DisposeHelper : IDisposable
        {
            readonly Action onDispose;
            bool isDisposed = false;

            public DisposeHelper(Action onDispose)
            {
                this.onDispose = onDispose;
            }

            public void Dispose()
            {
                if (isDisposed)
                    return;

                isDisposed = true;
                onDispose();
            }
        }
    }
}