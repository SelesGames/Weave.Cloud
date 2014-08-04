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

    //public class PubSubHelper
    //{
    //    ConnectionMultiplexer multiplexer;

    //    public PubSubHelper(ConnectionMultiplexer multiplexer)
    //    {
    //        this.multiplexer = multiplexer;
    //    }

    //    public async Task Publish(RedisChannel channel, RedisValue message)
    //    {
    //        var sb = multiplexer.GetSubscriber();
    //        var numClientsReceived = await sb.PublishAsync(
    //            channel: channel, 
    //            message: message, 
    //            flags: CommandFlags.None);
    //    }

    //    public Task Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler)
    //    {
    //        var sb = multiplexer.GetSubscriber();
    //        return sb.SubscribeAsync(
    //            channel: channel,
    //            handler: handler,
    //            flags: CommandFlags.None);
    //    }

    //    public async Task<IObservable<RedisPubSubTuple>> AsObservable(RedisChannel channel)
    //    {
    //        var observable = new ObservableRV();

    //        var sb = multiplexer.GetSubscriber();
    //        await sb.SubscribeAsync(
    //            channel: channel,
    //            handler: observable.Handler,
    //            flags: CommandFlags.None);

    //        return observable;
    //    }
    //}

    public class RedisPubSubTuple
    {
        internal RedisPubSubTuple(RedisChannel channel, RedisValue message)
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

    class ActionDisposable : IDisposable
    {
        Action onDispose; 
        public ActionDisposable(Action onDispose)
        {
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            if (onDispose != null)
                onDispose();
        }
    }
}
