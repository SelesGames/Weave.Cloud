using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;

namespace RedisDBHelper
{
    class RedisDeleteDatabase
    {
        readonly string dbNumber;

        public RedisDeleteDatabase(string dbNumber)
        {
            this.dbNumber = dbNumber;
        }

        public async Task<string> Execute()
        {
            int dbNum;
            if (!int.TryParse(dbNumber, out dbNum))
                return "not a valid db number";

            var connectionMultiplexer = Settings.ElevatedConnection;
            var server = connectionMultiplexer.GetServer(
"weaveuser.redis.cache.windows.net", 6379);
            await server.FlushDatabaseAsync(dbNum);
            return string.Format("db {0} deleted", dbNumber);
        }
    }
}