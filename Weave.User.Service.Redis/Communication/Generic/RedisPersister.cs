using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication.Generic
{
    public class RedisPersister<T1, T2> : IDisposable
    {
        readonly MessageQueue<T1> messageQueue;
        readonly Func<T1, Task<RedisCacheResult<T2>>> redisGet;
        readonly Func<T2, Task<bool>> persisterFunc;
        readonly TimeSpan pollingFrequency;

        IDisposable disposeHandle;

        public RedisPersister(
            MessageQueue<T1> messageQueue,
            Func<T1, Task<RedisCacheResult<T2>>> redisGet,
            Func<T2, Task<bool>> persisterFunc,
            TimeSpan pollingFrequency)
        {
            this.messageQueue = messageQueue;
            this.redisGet = redisGet;
            this.persisterFunc = persisterFunc;
            this.pollingFrequency = pollingFrequency;
        }

        // switch to an adaptive polling strategy, adapting based on whether there was a message
        public void Initialize()
        {
            disposeHandle = messageQueue.Observe(pollingFrequency, ProcessMessage);
        }

        async Task ProcessMessage(T1 message)
        {
            var redisResult = await redisGet(message);
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