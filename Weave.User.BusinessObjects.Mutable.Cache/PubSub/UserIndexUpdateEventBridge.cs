using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class UserIndexUpdateEventBridge
    {
        const string CHANNEL = "userIndexUpdate";

        readonly ConnectionMultiplexer cm;

        public UserIndexUpdateEventBridge(ConnectionMultiplexer cm)
        {
            this.cm = cm;
        }

        public async Task<long> Publish(UserIndexUpdateNotice notice)
        {
            var bytes = notice.WriteToBytes();

            var sub = cm.GetSubscriber();
            var received = await sub.PublishAsync(CHANNEL, bytes);
            return received;
        }

        public async Task<IDisposable> Observe(Action<UserIndexUpdateNotice> onNoticeReceived)
        {
            Action<RedisChannel, RedisValue> handler = (c, v) => 
                onNoticeReceived(v.ReadUserIndexUpdateNotice());

            var sub = cm.GetSubscriber();
            await sub.SubscribeAsync(
                channel: CHANNEL,
                handler: handler,
                flags: CommandFlags.None);

            return new DisposeHelper(async () => { try { await sub.UnsubscribeAsync(CHANNEL, handler); } catch { } });
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