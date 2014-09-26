using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Synchronization
{
    public static class IDatabaseLockExtensions
    {
        public static async Task<byte[]> Lock(
            this IDatabase db,
            RedisKey key,
            TimeSpan acquisitionTimeout,
            TimeSpan ttl,
            TimeSpan retryInterval)
        {
            Validate(acquisitionTimeout, "acquisitionTimeout");
            Validate(ttl, "ttl");
            Validate(retryInterval, "retryInterval");

            var lockToken = Guid.NewGuid().ToByteArray();

            bool wasLockSuccesfullyAcquired = false;

            var lockTimeout = LockTimeout.Create(acquisitionTimeout);
            while (!lockTimeout.HasTimedOut)
            {
                wasLockSuccesfullyAcquired = await db.StringSetAsync(
                    key: key,
                    value: lockToken,
                    expiry: ttl,
                    when: When.NotExists,
                    flags: CommandFlags.None);

                if (wasLockSuccesfullyAcquired)
                    return lockToken;

                else
                    await Task.Delay(retryInterval);
            }

            // if we got here, the lock acquisition timed out
            throw new RedisLockException(key);
        }

        static void Validate(TimeSpan timeSpan, string parameterName)
        {
            if (timeSpan <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(parameterName, "must be greater than zero");
        }

        static readonly string unlockLuaScript =
"if redis.call(\"get\",KEYS[1]) == ARGV[1]\r\n" +
"then\r\n" +
    "\treturn redis.call(\"del\",KEYS[1])\r\n" +
"else\r\n" +
    "\treturn 0\r\n" +
"end";

        public static async Task<bool> Unlock(this IDatabase db, RedisKey key, byte[] lockToken)
        {
            var keys = new[] { key };
            var args = new[] { (RedisValue)lockToken };
            
            var result = await db.ScriptEvaluateAsync(
                script: unlockLuaScript,
                keys: keys,
                values: args,
                flags: CommandFlags.None // | CommandFlags.FireAndForget enable FireAndForget for faster performance on unlocks
            );

            var numKeysRemoved = (long)result;

            return numKeysRemoved > 0;
        }
    }
}