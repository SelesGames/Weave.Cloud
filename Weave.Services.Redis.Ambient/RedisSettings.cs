using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.Services.Redis.Ambient
{
    internal class RedisSettings
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        public ConnectionMultiplexer Create()
        {
            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);
            return connectionMultiplexer;
        }

    }
}
