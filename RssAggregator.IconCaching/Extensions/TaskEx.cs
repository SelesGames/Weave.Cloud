using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weave.User.Service.Cache.Extensions
{
    internal static class TaskEx
    {
        public static async Task Retry(Func<Task> func, int maxAttempts, TimeSpan waitTime)
        {
            int retryCount = 0;
            List<Exception> exceptions = new List<Exception>();

            while (retryCount < maxAttempts)
            {
                try
                {
                    await func();
                    return;
                }
                catch(Exception e)
                {
                    exceptions.Add(e);
                }

                retryCount++;
                await Task.Delay(waitTime);
            }

            throw new AggregateException("retry count exceeded", exceptions);
        }
    }
}
