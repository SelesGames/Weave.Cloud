using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Testing
{
    class Program
    {
        static  void Main(string[] args)
        {
            //DoStuff().Wait();
        }

        static async Task TestJsonSerialization()
        {
            var redisClientConfig = ConfigurationOptions.Parse(
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=");

            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);
        }

        static async Task DoStuff()
        {
            var multiplexer = await ConnectionMultiplexer.ConnectAsync("serverUrl");
            var db = multiplexer.GetDatabase(0);

            SortedSetEntry[] values = new SortedSetEntry[] 
            { 
                new SortedSetEntry("hello hi", 67),
                new SortedSetEntry("hello hi", 99),
            };
            await db.SortedSetAddAsync("key", values);
        }
    }
}
