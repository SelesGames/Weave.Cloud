using StackExchange.Redis;
using System;
using System.Threading;

namespace Weave.Services.Redis.Ambient
{
    public static class Settings
    {
        static RedisSettings settings = new RedisSettings();

        public static ConnectionMultiplexer StandardConnection { get { return settings.StandardConnection; } }
        public static ConnectionMultiplexer PubsubConnection { get { return settings.PubsubConnection; } }
        public static ConnectionMultiplexer ElevatedConnection { get { return settings.ElevatedConnection; } }
    }

    class RedisSettings
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        Lazy<ConnectionMultiplexer> standardConnection;
        Lazy<ConnectionMultiplexer> pubsubConnection;
        Lazy<ConnectionMultiplexer> elevatedConnection;

        public RedisSettings()
        {
            standardConnection = new Lazy<ConnectionMultiplexer>(CreateStandard, LazyThreadSafetyMode.ExecutionAndPublication);
            pubsubConnection = new Lazy<ConnectionMultiplexer>(CreatePubSub, LazyThreadSafetyMode.ExecutionAndPublication);
            elevatedConnection = new Lazy<ConnectionMultiplexer>(CreateElevated, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        ConnectionMultiplexer CreateStandard()
        {
            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);
            return connectionMultiplexer;
        }

        ConnectionMultiplexer CreatePubSub()
        {
            var cm = CreateStandard();
            cm.PreserveAsyncOrder = false;
            return cm;
        }

        ConnectionMultiplexer CreateElevated()
        {
            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            redisClientConfig.AllowAdmin = true;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);
            return connectionMultiplexer;
        }

        public ConnectionMultiplexer StandardConnection { get { return standardConnection.Value; } }
        public ConnectionMultiplexer PubsubConnection { get { return pubsubConnection.Value; } }
        public ConnectionMultiplexer ElevatedConnection { get { return elevatedConnection.Value; } }
    }
}
