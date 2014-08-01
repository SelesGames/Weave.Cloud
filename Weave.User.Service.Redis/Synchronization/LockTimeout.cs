using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Synchronization
{
    class LockTimeout
    {
        readonly TimeSpan delay;
        bool hasTimedOut = false;

        public bool HasTimedOut { get { return hasTimedOut; } }

        LockTimeout(TimeSpan delay)
        {
            this.delay = delay;
        }

        async void BeginTimer()
        {
            await Task.Delay(delay);
            hasTimedOut = true;
        }

        public static LockTimeout Create(TimeSpan delay)
        {
            var o = new LockTimeout(delay);
            o.BeginTimer();
            return o;
        }
    }
}
