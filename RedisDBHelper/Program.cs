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
    public class Program
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        //static void Main(string[] args)
        //{
        //    //TestFeedUpdateToAzure().Wait();
        //    RunLoop().Wait();
        //}

        //static async Task RunLoop()
        //{
        //    while (true)
        //    {
        //        var input = Console.ReadLine();
                
        //    }
        //}

        public Task<string> ProcessInput(string input)
        {
            var parameters = input.Split(' ');
            var command = parameters.First();
            if (command.Equals("testfeed", StringComparison.OrdinalIgnoreCase))
            {
                var url = parameters.Skip(1).First();
                return new RedisFeedUpdaterTest(url).Execute();
            }

            else if (command.Equals("testentry", StringComparison.OrdinalIgnoreCase))
            {
                var id = parameters.Skip(1).First();
                return new RedisExpandedEntryTest(id).Execute();
            }

            else if (command.Equals("remfeed"))
            {
                var url = parameters.Skip(1).First();
                return new RedisDeleteFeedUpdater(url).Execute();
            }

            else return Task.FromResult("unrecognized command");
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
            var feedUrl = "http://www.polygon.com/rss/index.xml";

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