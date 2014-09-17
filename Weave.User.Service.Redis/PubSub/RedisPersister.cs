using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.PubSub
{
    public class RedisPersister<T1, T2> : IDisposable
    {
        readonly RedisPubSubObserver<T1> observer;
        readonly Func<T1, Task<RedisCacheResult<T2>>> redisGet;
        readonly Func<T2, Task<bool>> persisterFunc;
        IDisposable disposeHandle;

        public RedisPersister(
            RedisPubSubObserver<T1> observer,
            Func<T1, Task<RedisCacheResult<T2>>> redisGet,
            Func<T2, Task<bool>> persisterFunc)
        {
            this.observer = observer;
            this.redisGet = redisGet;
            this.persisterFunc = persisterFunc;
        }

        public async Task Initialize()
        {
            disposeHandle = await observer.Observe(OnNoticeReceived);
        }

        async void OnNoticeReceived(T1 notice)
        {
            var redisResult = await redisGet(notice);
            if (redisResult.HasValue)
            {
                var latest = redisResult.Value;
                var saveResult = await persisterFunc(latest);
            }
        }

        public void Dispose()
        {
            if (disposeHandle != null)
                disposeHandle.Dispose();
        }
    }
}