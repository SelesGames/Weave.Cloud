using System;

namespace Weave.User.Service.Redis
{
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