using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Synchronization.UserIndex
{
    public class UserLockHelper
    {
        readonly TimingHelper sw;
        readonly UserLockCache userIndexLocker;

        public TimeSpan LockAcquisitionTime { get; private set; }
        public TimeSpan LockReleaseTime { get; private set; }

        public UserLockHelper(UserLockCache userIndexLocker)
        {
            this.userIndexLocker = userIndexLocker;
            this.sw = new TimingHelper();
        }

        public async Task<T> Lock<T>(Guid userId, Func<Task<T>> func)
        {
            sw.Start();
            var token = await Lock(userId);
            LockAcquisitionTime = sw.Record();

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

            sw.Start();
            var wasUnlocked = await Unlock(userId, token);
            LockReleaseTime = sw.Record();

            if (ex != null)
                throw ex;
            else
                return result;
        }

        public async Task Lock(Guid userId, Func<Task> func)
        {
            sw.Start();
            var token = await Lock(userId);
            LockAcquisitionTime = sw.Record();

            Exception ex = null;

            try
            {
                await func();
            }
            catch (Exception e)
            {
                ex = e;
            }

            sw.Start();
            var wasUnlocked = await Unlock(userId, token);
            LockReleaseTime = sw.Record();

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
