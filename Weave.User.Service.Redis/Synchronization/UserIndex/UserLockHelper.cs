using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Synchronization.UserIndex
{
    public class UserLockHelper
    {
        readonly UserLockCache userIndexLocker;

        public UserLockHelper(UserLockCache userIndexLocker)
        {
            this.userIndexLocker = userIndexLocker;
        }

        public async Task<T> Lock<T>(Guid userId, Func<Task<T>> func)
        {
            var token = await Lock(userId);

            T result = default(T);
            Exception ex = null;

            try
            {
                result = await func();
            }
            catch (Exception e)
            {
                ex = e;
            }

            var wasUnlocked = await Unlock(userId, token);

            if (ex != null)
                throw ex;
            else
                return result;
        }

        public async Task Lock(Guid userId, Func<Task> func)
        {
            var token = await Lock(userId);

            Exception ex = null;

            try
            {
                await func();
            }
            catch (Exception e)
            {
                ex = e;
            }

            var wasUnlocked = await Unlock(userId, token);

            if (ex != null)
                throw ex;
        }

        Task<byte[]> Lock(Guid userId)
        {
            return userIndexLocker.Lock(userId);
        }

        Task<bool> Unlock(Guid userId, byte[] lockToken)
        {
            return userIndexLocker.Unlock(userId, lockToken);
        }
    }
}
