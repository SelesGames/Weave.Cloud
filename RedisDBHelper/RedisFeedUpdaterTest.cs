using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Service.Redis;
using Newtonsoft.Json;
using Weave.Services.Redis.Ambient;

namespace RedisDBHelper
{
    class RedisFeedUpdaterTest
    {
        readonly string url;

        public RedisFeedUpdaterTest(string url)
        {
            this.url = url;
        }

        public async Task<string> Execute()
        {
            var cm = Settings.StandardConnection;
            var db = cm.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var redisCache = new Weave.User.Service.Redis.Clients.FeedUpdaterCache(db);
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