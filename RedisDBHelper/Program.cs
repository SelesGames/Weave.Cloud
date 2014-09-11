using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Updater.Azure;
using Weave.User.Service.Redis;

namespace RedisDBHelper
{
    class Program
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        static void Main(string[] args)
        {
            while (true)
            {
                var input = Console.ReadLine();
                var parameters = input.Split(' ');
                var command = parameters.First();
                if (command.Equals("testfeed", StringComparison.OrdinalIgnoreCase))
                {
                    var url = parameters.Skip(1).First();
                    var temp = new RedisFeedUpdaterTest(url);
                    Console.WriteLine("executing...");
                    temp.Execute().Wait();
                    Console.WriteLine("finished.  check debug window");
                }
            }
        }

        static void Stuff()
        {
            var redisClientConfig = ConfigurationOptions.Parse(
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=");

            redisClientConfig.AllowAdmin = true;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);

            var server = connectionMultiplexer.GetServer(
"weaveuser.redis.cache.windows.net", 6379);
            server.FlushDatabase(0);
        }

        static async Task TestFeedUpdateToAzure()
        {
            var feedUrl = "http://www.gameinformer.com:80/feeds/thefeedrss.aspx";

            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            var cm = ConnectionMultiplexer.Connect(redisClientConfig);
            var db = cm.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var redisCache = new Weave.User.Service.Redis.FeedUpdaterCache(db);
            var blobClient = new Weave.Updater.Azure.FeedUpdaterCache(
                "weaveuser2",
                "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                "updaterfeeds");
            var blobUpdater = new FeedBlobUpdater(blobClient, redisCache);

            var wasUpdated = await blobUpdater.Update(feedUrl);
            Debug.WriteLine(wasUpdated);
        }
    }
}