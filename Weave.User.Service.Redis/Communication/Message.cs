using StackExchange.Redis;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis.Communication
{
    public class Message
    {
        readonly IDatabaseAsync db;
        readonly RedisKey processList;
        readonly RedisValue value;

        public RedisValue Value { get { return value; } }

        internal Message(IDatabaseAsync db, RedisKey processList, RedisValue value)
        {
            this.db = db;
            this.processList = processList;
            this.value = value;
        }

        public async Task<bool> Complete()
        {
            var numRemoved = await db.ListRemoveAsync(
                key: processList,
                value: value,
                count: 0,
                flags: CommandFlags.None
            );

            return numRemoved > 0;
        }
    }
}