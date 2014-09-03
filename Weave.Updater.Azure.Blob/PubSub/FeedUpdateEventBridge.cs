using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.Updater.PubSub
{
    public class FeedUpdateEventBridge
    {
        const string CHANNEL = "feedUpdate";

        readonly ConnectionMultiplexer cm;

        public FeedUpdateEventBridge(ConnectionMultiplexer cm)
        {
            this.cm = cm;
        }

        public Task<long> Publish(FeedUpdate update)
        {
            var notice = new FeedUpdateNotice { FeedUri = update.Feed.Uri, RefreshTime = update.RefreshTime };
            return Publish(notice);
        }

        public async Task<long> Publish(FeedUpdateNotice notice)
        {
            var bytes = notice.WriteToBytes();

            var sub = cm.GetSubscriber();
            var received = await sub.PublishAsync(CHANNEL, bytes);
            return received;
        }

        public async Task<IDisposable> Observe(Action<FeedUpdateNotice> onNoticeReceived)
        {
            Action<RedisChannel, RedisValue> handler = (c, v) => 
                onNoticeReceived(v.ReadFeedUpdateNotice());

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