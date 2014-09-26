using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Synchronization.UserIndex
{
    public class UserLockCache
    {
        readonly ConnectionMultiplexer connection;

        static readonly TimeSpan lockAcquisitionTimeout = TimeSpan.FromSeconds(5);
        static readonly TimeSpan lockTTL = TimeSpan.FromSeconds(4);
        static readonly TimeSpan lockRetryInterval = TimeSpan.FromMilliseconds(4);

        public UserLockCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<byte[]> Lock(Guid userId)
        {
            var db = connection.GetDatabase(DatabaseNumbers.LOCK);
            var key = userId.ToByteArray();

            try
            {
                var lockToken = await db.Lock(key, lockAcquisitionTimeout, lockTTL, lockRetryInterval);
                return lockToken;
            }
            catch(RedisLockException ex)
            {
                throw new Exception("unable to acquire lock on userid: " + userId.ToString(), ex);
            }
        }

        public Task<bool> Unlock(Guid userId, byte[] lockToken)
        {
            var db = connection.GetDatabase(DatabaseNumbers.LOCK);
            var key = userId.ToByteArray();        

            return db.Unlock(key, lockToken);
        }
    }
}