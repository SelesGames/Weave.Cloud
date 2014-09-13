using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Service.Redis;
using Newtonsoft.Json;

namespace RedisDBHelper
{
    class RedisConfig
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        public ConnectionMultiplexer CreateConnection()
        {
            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            var cm = ConnectionMultiplexer.Connect(redisClientConfig);
            return cm;
        }
    }

    class RedisFeedUpdaterTest
    {
        readonly string url;

        public RedisFeedUpdaterTest(string url)
        {
            this.url = url;
        }

        public async Task<string> Execute()
        {
            var config = new RedisConfig();
            var db = config.CreateConnection().GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var redisCache = new Weave.User.Service.Redis.FeedUpdaterCache(db);
            var result = await redisCache.Get(url);
            var val = result.Value;
            return val.Dump();
        }
    }

    public static class Dumper
    {
        public static string Dump(this object value)
        {
            var settings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };         
            return JsonConvert.SerializeObject(value, Formatting.Indented, settings);
        }
    }
}
