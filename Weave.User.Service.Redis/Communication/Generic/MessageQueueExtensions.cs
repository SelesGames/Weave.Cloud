using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication.Generic
{
    public static class MessageQueueExtensions
    {
        public static IDisposable Observe<T>(this MessageQueue<T> queue, 
            TimeSpan pollFrequency, 
            Func<T, Task> onMessageReceived, 
            Action<Exception> onException = null)
        {
            var isDisposed = false;
            var disposeHandle = new ActionDisposable(() => isDisposed = true);

            Task.Run(async () =>
            {
                while (true)
                {
                    if (isDisposed)
                        return;

                    Message<T> message = null;
                    Exception fault = null;

                    try
                    {
                        var next = await queue.GetNext();
                        if (next.IsSome)
                        {
                            message = next.Value;
                            var val = message.Value;
                            await onMessageReceived(val);
                        }
                    }
                    catch(Exception e) { fault = e; }

                    if (message != null)
                        await message.Complete();

                    if (fault != null && onException != null)
                        onException(fault);

                    await Task.Delay(pollFrequency);
                }
            });

            return disposeHandle;
        }
    }
}