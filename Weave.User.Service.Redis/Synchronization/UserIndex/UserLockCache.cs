using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Synchronization.UserIndex
{
    public class UserLockCache
    {
        readonly ConnectionMultiplexer connection;

        static readonly TimeSpan lockAcquisitionTimeout = TimeSpan.FromSeconds(2);
        static readonly TimeSpan lockTTL = TimeSpan.FromSeconds(4);
        static readonly TimeSpan lockRetryInterval = TimeSpan.FromMilliseconds(20);

        public UserLockCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<byte[]> Lock(Guid userId)
        {
            var db = connection.GetDatabase(DatabaseNumbers.LOCK);
            var key = userId.ToByteArray();
            var lockToken = Guid.NewGuid().ToByteArray();

            bool wasLockSuccesfullyAcquired = false;

            var lockTimeout = LockTimeout.Create(lockAcquisitionTimeout);
            while (!lockTimeout.HasTimedOut)
            {
                wasLockSuccesfullyAcquired = await db.StringSetAsync(key, lockToken,
                    expiry: lockTTL,
                    when: When.NotExists,
                    flags: CommandFlags.HighPriority);

                if (wasLockSuccesfullyAcquired)
                    return lockToken;

                else
                    await Task.Delay(lockRetryInterval);
            }

            // if we got here, the lock timed out
            throw new Exception("unable to acquire lock on userid: " + userId.ToString());
        }


static readonly string unlockLuaScript =
"if redis.call(\"get\",KEYS[1]) == ARGV[1]\r\n" +
"then\r\n" +
    "\treturn redis.call(\"del\",KEYS[1])\r\n" +
"else\r\n" +
    "\treturn 0\r\n" +
"end";

        public async Task<bool> Unlock(Guid userId, byte[] lockToken)
        {
            var keys = new[] { (RedisKey)userId.ToByteArray() };
            var args = new[] { (RedisValue)lockToken };
            
            var db = connection.GetDatabase(DatabaseNumbers.LOCK);
            var result = await db.ScriptEvaluateAsync(
                script: unlockLuaScript,
                keys: keys,
                values: args,
                flags: CommandFlags.None // | CommandFlags.FireAndForget enable FireAndForget for faster performance on unlocks
            );

            var numKeysRemoved = (long)result;

            return numKeysRemoved > 0;

            //var result = await db.KeyDeleteAsync(
            //    keys[0],
            //    flags: CommandFlags.None);

            //return result;
        }
    }
}
