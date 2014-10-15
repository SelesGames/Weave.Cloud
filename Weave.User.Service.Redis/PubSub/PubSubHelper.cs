using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.PubSub
{
    public static class PubSubExtensions
    {
        public static async Task<IObservable<RedisPubSubTuple>> AsObservable(this ISubscriber sb, RedisChannel channel)
        {
            var observable = new ObservableRV();

            await sb.SubscribeAsync(
                channel: channel,
                handler: observable.Handler,
                flags: CommandFlags.None);

            return observable;
        }
    }

    public class RedisPubSubTuple
    {
        public RedisPubSubTuple(RedisChannel channel, RedisValue message)
        {
            Channel = channel;
            Message = message;
        }

        public RedisChannel Channel { get; set; }
        public RedisValue Message { get; set; }
    }

    class ObservableRV : IObservable<RedisPubSubTuple>
    {
        IObserver<RedisPubSubTuple> valueObserver;

        public void Handler(RedisChannel channel, RedisValue value)
        {
            if (valueObserver != null)
                valueObserver.OnNext(new RedisPubSubTuple(channel, value));
        }

        public IDisposable Subscribe(IObserver<RedisPubSubTuple> observer)
        {
            valueObserver = observer;
            return new ActionDisposable(() => valueObserver = null);
        }
    }
}
