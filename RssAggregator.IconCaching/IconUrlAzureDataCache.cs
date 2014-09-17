using Common.Caching;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;

namespace RssAggregator.IconCaching
{
    public class IconUrlAzureDataCache : IExtendedCache<string, Task<string>>
    {
        readonly string ICON_PREFIX = "FEEDICON:";

        readonly ConnectionMultiplexer connection;

        public IconUrlAzureDataCache()
        {
            connection = Settings.StandardConnection;
        }

        public async Task<string> GetOrAdd(string key, Func<string, Task<string>> valueFactory)
        {
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var redisKey = ICON_PREFIX + key;
            var result = await db.StringGetAsync(redisKey, CommandFlags.None);
            if (result.HasValue)
            {
                return (string)result;
            }
            else
            {
                var val = await valueFactory(key);

                var saveResult = await db.StringSetAsync(redisKey, val,
                    expiry: TimeSpan.FromDays(90),
                    when: When.Always,
                    flags: CommandFlags.None);

                return val;
            }
        }
    }
}